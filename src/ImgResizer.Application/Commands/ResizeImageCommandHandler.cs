using System.Globalization;
using ImgResizer.Application.DTOs;
using ImgResizer.Domain.Common;
using ImgResizer.Domain.Interfaces;
using ImgResizer.Infrastructure.Configuration;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImgResizer.Application.Commands;

/// <summary>
/// 画像リサイズコマンドのハンドラー。
/// MediatRを使用してCQRSパターンでコマンドを処理します。
/// </summary>
public class ResizeImageCommandHandler : IRequestHandler<ResizeImageCommand, Result<ResizeImageResponse>>
{
    private readonly IImageRepository _imageRepository;
    private readonly IImageResizeService _imageResizeService;
    private readonly IOptions<ImageResizeSettings> _settings;
    private readonly ILogger<ResizeImageCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResizeImageCommandHandler"/> class.
    /// </summary>
    /// <param name="imageRepository">画像リポジトリ。</param>
    /// <param name="imageResizeService">画像リサイズサービス。</param>
    /// <param name="settings">画像リサイズ設定。</param>
    /// <param name="logger">ロガー。</param>
    public ResizeImageCommandHandler(
        IImageRepository imageRepository,
        IImageResizeService imageResizeService,
        IOptions<ImageResizeSettings> settings,
        ILogger<ResizeImageCommandHandler> logger)
    {
        _imageRepository = imageRepository;
        _imageResizeService = imageResizeService;
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// 画像リサイズコマンドを処理します。
    /// </summary>
    /// <param name="request">リサイズコマンド。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>リサイズ処理結果。</returns>
    public async Task<Result<ResizeImageResponse>> Handle(
        ResizeImageCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "画像変換処理開始: FilePath={FilePath}, ResizeMode={ResizeMode}",
            request.FilePath,
            request.ResizeMode ?? "fit");

        var extension = Path.GetExtension(request.FilePath).ToLower(CultureInfo.InvariantCulture);

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

        _logger.LogInformation(
            "画像変換処理完了: OutputPath={OutputPath}, ResizeMode={ResizeMode}",
            outputPath,
            resizeMode);

        return Result.Success(new ResizeImageResponse
        {
            Success = true,
            Message = "画像を512×512に変換しました",
            OutputPath = outputPath,
            ResizeMode = resizeMode
        });
    }
}
