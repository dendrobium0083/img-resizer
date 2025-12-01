using FluentValidation;
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
    private readonly IValidator<ResizeImageRequest> _validator;

    public ResizeImageUseCase(
        IImageRepository imageRepository,
        IImageResizeService imageResizeService,
        IOptions<ImageResizeSettings> settings,
        ILogger<ResizeImageUseCase> logger,
        IValidator<ResizeImageRequest> validator)
    {
        _imageRepository = imageRepository;
        _imageResizeService = imageResizeService;
        _settings = settings;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<ResizeImageResponse>> ExecuteAsync(ResizeImageRequest request)
    {
        try
        {
            _logger.LogDebug("画像変換処理開始: FilePath={FilePath}, ResizeMode={ResizeMode}", 
                request.FilePath, request.ResizeMode ?? "fit");

            // FluentValidationによるバリデーション
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var firstError = validationResult.Errors.First();
                _logger.LogWarning("バリデーションエラー: {ErrorMessage}", firstError.ErrorMessage);
                return Result.Failure<ResizeImageResponse>(
                    firstError.ErrorCode,
                    firstError.ErrorMessage);
            }

            var extension = Path.GetExtension(request.FilePath).ToLower();

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
}

