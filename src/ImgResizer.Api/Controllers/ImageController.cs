using ImgResizer.Application.Commands;
using ImgResizer.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ImgResizer.Api.Controllers;

/// <summary>
/// 画像変換APIコントローラー。
/// MediatRを使用してCQRSパターンでリクエストを処理します。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ImageController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageController"/> class.
    /// </summary>
    /// <param name="mediator">MediatRメディエーター。</param>
    /// <param name="logger">ロガー。</param>
    public ImageController(
        IMediator mediator,
        ILogger<ImageController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// 画像を512×512の正方形に変換します。
    /// </summary>
    /// <param name="request">リクエスト。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>変換結果。</returns>
    [HttpPost("resize")]
    public async Task<ActionResult<ResizeImageResponse>> ResizeImage(
        [FromBody] ResizeImageRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "画像変換リクエスト受信: FilePath={FilePath}, ResizeMode={ResizeMode}",
            request.FilePath,
            request.ResizeMode ?? "fit");

        var command = new ResizeImageCommand(request.FilePath, request.ResizeMode);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("画像変換処理完了: OutputPath={OutputPath}", result.Value.OutputPath);
            return Ok(result.Value);
        }

        // エラーコードに基づいてHTTPステータスコードを決定
        var errorResponse = new ResizeImageResponse
        {
            Success = false,
            ErrorCode = result.ErrorCode,
            Message = result.ErrorMessage
        };

        return result.ErrorCode switch
        {
            "FILE_NOT_FOUND" => NotFound(errorResponse),
            "VALIDATION_ERROR" => BadRequest(errorResponse),
            "UNSUPPORTED_FORMAT" => BadRequest(errorResponse),
            "FILE_TOO_LARGE" => BadRequest(errorResponse),
            "FILE_READ_ERROR" => StatusCode(500, errorResponse),
            "FILE_WRITE_ERROR" => StatusCode(500, errorResponse),
            "IMAGE_LOAD_ERROR" => BadRequest(errorResponse),
            "IMAGE_PROCESSING_ERROR" => StatusCode(500, errorResponse),
            "INTERNAL_SERVER_ERROR" => StatusCode(500, errorResponse),
            _ => StatusCode(500, errorResponse)
        };
    }
}
