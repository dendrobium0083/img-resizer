using ImgResizer.Domain.Common;
using ImgResizer.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ImgResizer.Infrastructure.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;

namespace ImgResizer.Infrastructure.Services;

/// <summary>
/// 画像リサイズ処理の実装
/// </summary>
public class ImageResizeService : IImageResizeService
{
    private readonly ILogger<ImageResizeService> _logger;
    private readonly IOptions<ImageResizeSettings> _settings;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    /// <param name="settings">画像リサイズ設定</param>
    public ImageResizeService(
        ILogger<ImageResizeService> logger,
        IOptions<ImageResizeSettings> settings)
    {
        _logger = logger;
        _settings = settings;
    }

    /// <summary>
    /// 画像を正方形にリサイズする
    /// </summary>
    /// <param name="imageData">元画像データ（バイト配列）</param>
    /// <param name="size">ターゲットサイズ（512）</param>
    /// <param name="resizeMode">変換方式（fit または crop）</param>
    /// <param name="extension">画像の拡張子</param>
    /// <returns>リサイズ後の画像データ（バイト配列）を含むResult</returns>
    public async Task<Result<byte[]>> ResizeToSquareAsync(byte[] imageData, int size, string resizeMode, string extension)
    {
        return await Task.Run(() =>
        {
            try
            {
                _logger.LogDebug("画像リサイズ処理開始: Size={Size}, Mode={ResizeMode}", size, resizeMode);

                using var image = Image.Load(imageData);

                byte[] result;
                if (resizeMode == "crop")
                {
                    result = ResizeWithCrop(image, size, extension);
                }
                else
                {
                    result = ResizeWithFit(image, size, extension);
                }

                _logger.LogDebug("画像リサイズ処理完了: Size={Size}, Mode={ResizeMode}, ResultSize={ResultSize} bytes", 
                    size, resizeMode, result.Length);
                
                return Result.Success(result);
            }
            catch (UnknownImageFormatException ex)
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

    /// <summary>
    /// サポートされている画像形式か判定する
    /// </summary>
    /// <param name="filePath">ファイルパス</param>
    /// <returns>サポートされている場合true</returns>
    public bool IsSupportedFormat(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        var allowedExtensions = _settings.Value.AllowedExtensions;
        return allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 全体変換方式（fit）: アスペクト比を維持したリサイズとパディング
    /// </summary>
    private byte[] ResizeWithFit(Image image, int size, string extension)
    {
        // アスペクト比を計算
        double aspectRatio = (double)image.Width / image.Height;

        // リサイズサイズの決定
        int newWidth, newHeight;
        if (image.Width > image.Height)
        {
            newWidth = size;
            newHeight = (int)(size / aspectRatio);
        }
        else
        {
            newHeight = size;
            newWidth = (int)(size * aspectRatio);
        }

        // リサイズ処理（クローンを作成して処理）
        using var resizedImage = image.CloneAs<SixLabors.ImageSharp.PixelFormats.Rgba32>();
        resizedImage.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(newWidth, newHeight),
            Mode = ResizeMode.Max,
            Sampler = KnownResamplers.Lanczos3
        }));

        // 正方形キャンバスの作成
        using var squareCanvas = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(size, size);
        
        // パディング色で塗りつぶし
        var paddingColor = GetPaddingColor();
        squareCanvas.Mutate(x => x.BackgroundColor(paddingColor));

        // 中央配置
        int x = (size - newWidth) / 2;
        int y = (size - newHeight) / 2;
        squareCanvas.Mutate(ctx => ctx.DrawImage(resizedImage, new Point(x, y), 1f));

        // 画像データの変換
        return ConvertToByteArray(squareCanvas, extension);
    }

    /// <summary>
    /// 中央クロップ方式（crop）: 画像の中央部分を切り出してリサイズ
    /// </summary>
    private byte[] ResizeWithCrop(Image image, int size, string extension)
    {
        // 元画像が指定サイズより小さい場合は、そのまま拡大
        if (image.Width < size || image.Height < size)
        {
            using var resizedImage = image.CloneAs<SixLabors.ImageSharp.PixelFormats.Rgba32>();
            resizedImage.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(size, size),
                Mode = ResizeMode.Stretch,
                Sampler = KnownResamplers.Lanczos3
            }));
            return ConvertToByteArray(resizedImage, extension);
        }

        // クロップサイズの決定
        int cropSize = Math.Min(image.Width, image.Height);

        // 中央位置の計算
        int x = (image.Width - cropSize) / 2;
        int y = (image.Height - cropSize) / 2;

        // 中央部分の切り出しとリサイズ（クローンを作成して処理）
        using var croppedImage = image.CloneAs<SixLabors.ImageSharp.PixelFormats.Rgba32>();
        croppedImage.Mutate(ctx => ctx
            .Crop(new Rectangle(x, y, cropSize, cropSize))
            .Resize(new ResizeOptions
            {
                Size = new Size(size, size),
                Mode = ResizeMode.Stretch,
                Sampler = KnownResamplers.Lanczos3
            }));

        return ConvertToByteArray(croppedImage, extension);
    }

    /// <summary>
    /// パディング色を取得
    /// </summary>
    private SixLabors.ImageSharp.Color GetPaddingColor()
    {
        var paddingColorSettings = _settings.Value.PaddingColor;
        if (paddingColorSettings != null)
        {
            return SixLabors.ImageSharp.Color.FromRgba(
                (byte)paddingColorSettings.R,
                (byte)paddingColorSettings.G,
                (byte)paddingColorSettings.B,
                (byte)paddingColorSettings.A);
        }
        return SixLabors.ImageSharp.Color.Black;
    }

    /// <summary>
    /// 画像をバイト配列に変換
    /// </summary>
    private byte[] ConvertToByteArray(Image image, string extension)
    {
        using var memoryStream = new MemoryStream();
        IImageEncoder encoder = GetImageEncoder(extension);
        image.Save(memoryStream, encoder);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// 拡張子からIImageEncoderを取得
    /// </summary>
    private IImageEncoder GetImageEncoder(string extension)
    {
        return extension.ToLower() switch
        {
            ".jpg" or ".jpeg" => new JpegEncoder
            {
                Quality = _settings.Value.ImageQuality?.JpegQuality ?? 90
            },
            ".png" => new PngEncoder
            {
                CompressionLevel = (PngCompressionLevel)(_settings.Value.ImageQuality?.PngCompressionLevel ?? 6)
            },
            ".gif" => new GifEncoder(),
            ".bmp" => new BmpEncoder(),
            _ => new PngEncoder()
        };
    }
}
