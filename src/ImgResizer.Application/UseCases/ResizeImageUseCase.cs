using ImgResizer.Application.DTOs;
using ImgResizer.Domain.Exceptions;
using ImgResizer.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ImgResizer.Infrastructure.Configuration;

namespace ImgResizer.Application.UseCases;

/// <summary>
/// 画像を512×512にリサイズするユースケース
/// </summary>
public class ResizeImageUseCase
{
    private readonly IImageRepository _imageRepository;
    private readonly IImageResizeService _imageResizeService;
    private readonly IOptions<ImageResizeSettings> _settings;
    private readonly ILogger<ResizeImageUseCase> _logger;

    public ResizeImageUseCase(
        IImageRepository imageRepository,
        IImageResizeService imageResizeService,
        IOptions<ImageResizeSettings> settings,
        ILogger<ResizeImageUseCase> logger)
    {
        _imageRepository = imageRepository;
        _imageResizeService = imageResizeService;
        _settings = settings;
        _logger = logger;
    }

    public async Task<ResizeImageResponse> ExecuteAsync(ResizeImageRequest request)
    {
        try
        {
            _logger.LogDebug("画像変換処理開始: {FilePath}, ResizeMode: {ResizeMode}", 
                request.FilePath, request.ResizeMode ?? "fit");

            // バリデーション
            ValidateRequest(request);

            // ファイル存在確認
            if (!_imageRepository.FileExists(request.FilePath))
            {
                throw new Domain.Exceptions.FileNotFoundException(request.FilePath);
            }

            // 拡張子チェック
            var extension = Path.GetExtension(request.FilePath).ToLower();
            if (!_imageResizeService.IsSupportedFormat(request.FilePath))
            {
                throw new UnsupportedFormatException(extension);
            }

            // ファイルサイズチェック
            var fileInfo = new FileInfo(request.FilePath);
            if (fileInfo.Length > _settings.Value.MaxFileSize)
            {
                throw new FileTooLargeException(fileInfo.Length, _settings.Value.MaxFileSize);
            }

            // 画像読み込み
            byte[] imageData;
            try
            {
                imageData = await _imageRepository.ReadImageAsync(request.FilePath);
            }
            catch (Domain.Exceptions.FileNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FileReadException(request.FilePath, ex);
            }

            // 画像変換
            var resizeMode = request.ResizeMode ?? "fit";
            byte[] resizedImageData;
            try
            {
                resizedImageData = await _imageResizeService.ResizeToSquareAsync(
                    imageData, 
                    _settings.Value.TargetSize.Width, 
                    resizeMode,
                    extension);
            }
            catch (Exception ex)
            {
                throw new ImageProcessingErrorException("画像変換処理に失敗しました", ex);
            }

            // 出力パス生成
            var outputPath = _imageRepository.GetOutputPath(
                request.FilePath, 
                _settings.Value.OutputDirectory, 
                resizeMode);

            // 画像保存
            try
            {
                await _imageRepository.SaveImageAsync(outputPath, resizedImageData);
            }
            catch (Exception ex)
            {
                throw new FileWriteException(outputPath, ex);
            }

            _logger.LogInformation("画像変換処理完了: {OutputPath}, ResizeMode: {ResizeMode}", 
                outputPath, resizeMode);

            return new ResizeImageResponse
            {
                Success = true,
                Message = "画像を512×512に変換しました",
                OutputPath = outputPath,
                ResizeMode = resizeMode
            };
        }
        catch (ImageProcessingException)
        {
            // Domain層の例外はそのまま再スロー
            throw;
        }
        catch (Exception ex)
        {
            // 予期しない例外はラップ
            _logger.LogError(ex, "予期しないエラーが発生しました: {FilePath}", request.FilePath);
            throw new ImageProcessingException("INTERNAL_SERVER_ERROR",
                "予期しないエラーが発生しました", ex);
        }
    }

    private void ValidateRequest(ResizeImageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FilePath))
        {
            throw new ValidationException("ファイルパスが指定されていません");
        }

        // パストラバーサルチェック
        if (request.FilePath.Contains("..") || request.FilePath.Contains("~"))
        {
            throw new ValidationException("無効なファイルパスです");
        }

        // resizeModeの検証
        var validModes = new[] { "fit", "crop" };
        var resizeMode = request.ResizeMode ?? "fit";
        if (!validModes.Contains(resizeMode.ToLower()))
        {
            throw new ValidationException(
                $"無効な変換方式です: {resizeMode}。有効な値は 'fit' または 'crop' です。");
        }
    }
}

