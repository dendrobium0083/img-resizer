using ImgResizer.Application.DTOs;
using ImgResizer.Domain.Common;
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

    public async Task<Result<ResizeImageResponse>> ExecuteAsync(ResizeImageRequest request)
    {
        try
        {
            _logger.LogDebug("画像変換処理開始: FilePath={FilePath}, ResizeMode={ResizeMode}", 
                request.FilePath, request.ResizeMode ?? "fit");

            // バリデーション
            var validationResult = ValidateRequest(request);
            if (validationResult.IsFailure)
            {
                return Result.Failure<ResizeImageResponse>(
                    validationResult.ErrorCode, 
                    validationResult.ErrorMessage);
            }

            // ファイル存在確認
            if (!_imageRepository.FileExists(request.FilePath))
            {
                _logger.LogWarning("ファイルが見つかりません: {FilePath}", request.FilePath);
                return Result.Failure<ResizeImageResponse>(
                    "FILE_NOT_FOUND",
                    $"ファイルが見つかりません: {request.FilePath}");
            }

            // 拡張子チェック
            var extension = Path.GetExtension(request.FilePath).ToLower();
            if (!_imageResizeService.IsSupportedFormat(request.FilePath))
            {
                _logger.LogWarning("サポートされていない画像形式: {Extension}", extension);
                return Result.Failure<ResizeImageResponse>(
                    "UNSUPPORTED_FORMAT",
                    $"サポートされていない画像形式です: {extension}");
            }

            // ファイルサイズチェック
            var fileInfo = new FileInfo(request.FilePath);
            if (fileInfo.Length > _settings.Value.MaxFileSize)
            {
                _logger.LogWarning("ファイルサイズ超過: {FileSize} bytes (上限: {MaxFileSize} bytes)", 
                    fileInfo.Length, _settings.Value.MaxFileSize);
                return Result.Failure<ResizeImageResponse>(
                    "FILE_TOO_LARGE",
                    $"ファイルサイズが大きすぎます。上限: {_settings.Value.MaxFileSize / (1024 * 1024)}MB");
            }

            // 画像読み込み
            var imageDataResult = await _imageRepository.ReadImageAsync(request.FilePath);
            if (imageDataResult.IsFailure)
            {
                return Result.Failure<ResizeImageResponse>(
                    imageDataResult.ErrorCode,
                    imageDataResult.ErrorMessage);
            }

            // 画像変換
            var resizeMode = request.ResizeMode ?? "fit";
            var resizedImageDataResult = await _imageResizeService.ResizeToSquareAsync(
                imageDataResult.Value, 
                _settings.Value.TargetSize.Width, 
                resizeMode,
                extension);

            if (resizedImageDataResult.IsFailure)
            {
                return Result.Failure<ResizeImageResponse>(
                    resizedImageDataResult.ErrorCode,
                    resizedImageDataResult.ErrorMessage);
            }

            // 出力パス生成
            var outputPath = _imageRepository.GetOutputPath(
                request.FilePath, 
                _settings.Value.OutputDirectory, 
                resizeMode);

            // 画像保存
            var saveResult = await _imageRepository.SaveImageAsync(outputPath, resizedImageDataResult.Value);
            if (saveResult.IsFailure)
            {
                return Result.Failure<ResizeImageResponse>(
                    saveResult.ErrorCode,
                    saveResult.ErrorMessage);
            }

            _logger.LogInformation("画像変換処理完了: OutputPath={OutputPath}, ResizeMode={ResizeMode}", 
                outputPath, resizeMode);

            return Result.Success(new ResizeImageResponse
            {
                Success = true,
                Message = "画像を512×512に変換しました",
                OutputPath = outputPath,
                ResizeMode = resizeMode
            });
        }
        catch (Exception ex)
        {
            // 予期しない例外をキャッチ
            _logger.LogError(ex, "予期しないエラーが発生しました: {FilePath}", request.FilePath);
            return Result.Failure<ResizeImageResponse>(
                "INTERNAL_SERVER_ERROR",
                "予期しないエラーが発生しました");
        }
    }

    private Result ValidateRequest(ResizeImageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FilePath))
        {
            _logger.LogWarning("バリデーションエラー: ファイルパスが指定されていません");
            return Result.Failure("VALIDATION_ERROR", "ファイルパスが指定されていません");
        }

        // パストラバーサルチェック
        if (request.FilePath.Contains("..") || request.FilePath.Contains("~"))
        {
            _logger.LogWarning("バリデーションエラー: 無効なファイルパス: {FilePath}", request.FilePath);
            return Result.Failure("VALIDATION_ERROR", "無効なファイルパスです");
        }

        // resizeModeの検証
        var validModes = new[] { "fit", "crop" };
        var resizeMode = request.ResizeMode ?? "fit";
        if (!validModes.Contains(resizeMode.ToLower()))
        {
            _logger.LogWarning("バリデーションエラー: 無効な変換方式: {ResizeMode}", resizeMode);
            return Result.Failure(
                "VALIDATION_ERROR",
                $"無効な変換方式です: {resizeMode}。有効な値は 'fit' または 'crop' です。");
        }

        return Result.Success();
    }
}

