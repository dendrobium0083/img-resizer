using System.Drawing;
using System.Drawing.Imaging;
using ImgResizer.Domain.Common;
using ImgResizer.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ImgResizer.Infrastructure.Configuration;

namespace ImgResizer.Infrastructure.Services;

/// <summary>
/// 画像リサイズ処理の実装
/// </summary>
public class ImageResizeService : IImageResizeService
{
    private readonly ILogger<ImageResizeService> _logger;
    private readonly IOptions<ImageResizeSettings> _settings;

    public ImageResizeService(
        ILogger<ImageResizeService> logger,
        IOptions<ImageResizeSettings> settings)
    {
        _logger = logger;
        _settings = settings;
    }

    public async Task<Result<byte[]>> ResizeToSquareAsync(byte[] imageData, int size, string resizeMode, string extension)
    {
        return await Task.Run(() =>
        {
            try
            {
                _logger.LogDebug("画像リサイズ処理開始: Size={Size}, Mode={ResizeMode}", size, resizeMode);

                using var originalImageStream = new MemoryStream(imageData);
                using var originalImage = new Bitmap(originalImageStream);

                byte[] result;
                if (resizeMode == "crop")
                {
                    result = ResizeWithCrop(originalImage, size, extension);
                }
                else
                {
                    result = ResizeWithFit(originalImage, size, extension);
                }

                _logger.LogDebug("画像リサイズ処理完了: Size={Size}, Mode={ResizeMode}, ResultSize={ResultSize} bytes", 
                    size, resizeMode, result.Length);
                
                return Result.Success(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "無効な画像データ: Mode={ResizeMode}", resizeMode);
                return Result.Failure<byte[]>(
                    "IMAGE_LOAD_ERROR",
                    "画像データの読み込みに失敗しました。画像ファイルが破損している可能性があります。");
            }
            catch (OutOfMemoryException ex)
            {
                _logger.LogError(ex, "メモリ不足エラー: Mode={ResizeMode}", resizeMode);
                return Result.Failure<byte[]>(
                    "IMAGE_PROCESSING_ERROR",
                    "画像のリサイズ処理中にメモリ不足が発生しました。");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "画像リサイズ処理エラー: Mode={ResizeMode}", resizeMode);
                return Result.Failure<byte[]>(
                    "IMAGE_PROCESSING_ERROR",
                    $"画像リサイズ処理に失敗しました: {ex.Message}");
            }
        });
    }

    public bool IsSupportedFormat(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        var allowedExtensions = _settings.Value.AllowedExtensions;
        return allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 全体変換方式（fit）: アスペクト比を維持したリサイズとパディング
    /// </summary>
    private byte[] ResizeWithFit(Bitmap originalImage, int size, string extension)
    {
        // アスペクト比を計算
        double aspectRatio = (double)originalImage.Width / originalImage.Height;

        // リサイズサイズの決定
        int newWidth, newHeight;
        if (originalImage.Width > originalImage.Height)
        {
            newWidth = size;
            newHeight = (int)(size / aspectRatio);
        }
        else
        {
            newHeight = size;
            newWidth = (int)(size * aspectRatio);
        }

        // リサイズ処理
        using var resizedImage = new Bitmap(newWidth, newHeight);
        using (var graphics = Graphics.FromImage(resizedImage))
        {
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
        }

        // 正方形キャンバスの作成
        using var squareCanvas = new Bitmap(size, size);
        using (var graphics = Graphics.FromImage(squareCanvas))
        {
            // パディング色で塗りつぶし
            var paddingColor = GetPaddingColor();
            graphics.Clear(paddingColor);

            // 中央配置
            int x = (size - newWidth) / 2;
            int y = (size - newHeight) / 2;
            graphics.DrawImage(resizedImage, x, y, newWidth, newHeight);
        }

        // 画像データの変換
        return ConvertToByteArray(squareCanvas, extension);
    }

    /// <summary>
    /// 中央クロップ方式（crop）: 画像の中央部分を切り出してリサイズ
    /// </summary>
    private byte[] ResizeWithCrop(Bitmap originalImage, int size, string extension)
    {
        // 元画像が512×512より小さい場合は、そのまま拡大
        if (originalImage.Width < size || originalImage.Height < size)
        {
            using var resizedImage = new Bitmap(size, size);
            using (var graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.DrawImage(originalImage, 0, 0, size, size);
            }
            return ConvertToByteArray(resizedImage, extension);
        }

        // クロップサイズの決定
        int cropSize = Math.Min(originalImage.Width, originalImage.Height);

        // 中央位置の計算
        int x = (originalImage.Width - cropSize) / 2;
        int y = (originalImage.Height - cropSize) / 2;

        // 中央部分の切り出し
        var cropRect = new Rectangle(x, y, cropSize, cropSize);
        using var croppedImage = originalImage.Clone(cropRect, originalImage.PixelFormat);

        // リサイズ処理
        using var finalImage = new Bitmap(size, size);
        using (var graphics = Graphics.FromImage(finalImage))
        {
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.DrawImage(croppedImage, 0, 0, size, size);
        }

        return ConvertToByteArray(finalImage, extension);
    }

    /// <summary>
    /// パディング色を取得
    /// </summary>
    private Color GetPaddingColor()
    {
        var paddingColorSettings = _settings.Value.PaddingColor;
        if (paddingColorSettings != null)
        {
            return Color.FromArgb(
                paddingColorSettings.A,
                paddingColorSettings.R,
                paddingColorSettings.G,
                paddingColorSettings.B);
        }
        return Color.Black;
    }

    /// <summary>
    /// 画像をバイト配列に変換
    /// </summary>
    private byte[] ConvertToByteArray(Bitmap image, string extension)
    {
        using var memoryStream = new MemoryStream();
        var imageFormat = GetImageFormat(extension);
        
        // JPEG品質設定
        if (imageFormat == ImageFormat.Jpeg && _settings.Value.ImageQuality != null)
        {
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(
                System.Drawing.Imaging.Encoder.Quality, 
                (long)_settings.Value.ImageQuality.JpegQuality);
            
            var jpegCodec = ImageCodecInfo.GetImageEncoders()
                .FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
            
            if (jpegCodec != null)
            {
                image.Save(memoryStream, jpegCodec, encoderParameters);
                encoderParameters.Dispose();
                return memoryStream.ToArray();
            }
        }

        image.Save(memoryStream, imageFormat);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// 拡張子からImageFormatを取得
    /// </summary>
    private ImageFormat GetImageFormat(string extension)
    {
        return extension.ToLower() switch
        {
            ".jpg" or ".jpeg" => ImageFormat.Jpeg,
            ".png" => ImageFormat.Png,
            ".gif" => ImageFormat.Gif,
            ".bmp" => ImageFormat.Bmp,
            _ => ImageFormat.Png
        };
    }
}

