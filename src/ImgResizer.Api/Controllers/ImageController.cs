using ImgResizer.Application.DTOs;
using ImgResizer.Application.UseCases;
using ImgResizer.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ImgResizer.Api.Controllers;

/// <summary>
/// 画像変換APIコントローラー
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly ResizeImageUseCase _resizeImageUseCase;
    private readonly ILogger<ImageController> _logger;

    public ImageController(
        ResizeImageUseCase resizeImageUseCase,
        ILogger<ImageController> logger)
    {
        _resizeImageUseCase = resizeImageUseCase;
        _logger = logger;
    }

    /// <summary>
    /// 画像を512×512の正方形に変換する
    /// </summary>
    /// <param name="request">リクエスト</param>
    /// <returns>変換結果</returns>
    [HttpPost("resize")]
    public async Task<ActionResult<ResizeImageResponse>> ResizeImage(
        [FromBody] ResizeImageRequest request)
    {
        try
        {
            _logger.LogInformation("画像変換リクエスト受信: {FilePath}, ResizeMode: {ResizeMode}", 
                request.FilePath, request.ResizeMode ?? "fit");

            var response = await _resizeImageUseCase.ExecuteAsync(request);

            if (response.Success)
            {
                _logger.LogInformation("画像変換処理完了: {OutputPath}", response.OutputPath);
                return Ok(response);
            }

            _logger.LogWarning("画像変換処理失敗: {ErrorCode}, {Message}", 
                response.ErrorCode, response.Message);
            return BadRequest(response);
        }
        catch (Domain.Exceptions.FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "ファイルが見つかりません: {FilePath}", request.FilePath);
            return NotFound(new ResizeImageResponse
            {
                Success = false,
                Message = ex.Message,
                ErrorCode = ex.ErrorCode
            });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "バリデーションエラー: {Message}", ex.Message);
            return BadRequest(new ResizeImageResponse
            {
                Success = false,
                Message = ex.Message,
                ErrorCode = ex.ErrorCode
            });
        }
        catch (UnsupportedFormatException ex)
        {
            _logger.LogWarning(ex, "サポートされていない画像形式: {Message}", ex.Message);
            return BadRequest(new ResizeImageResponse
            {
                Success = false,
                Message = ex.Message,
                ErrorCode = ex.ErrorCode
            });
        }
        catch (FileTooLargeException ex)
        {
            _logger.LogWarning(ex, "ファイルサイズ超過: {Message}", ex.Message);
            return BadRequest(new ResizeImageResponse
            {
                Success = false,
                Message = ex.Message,
                ErrorCode = ex.ErrorCode
            });
        }
        catch (ImageProcessingException ex)
        {
            _logger.LogError(ex, "画像処理エラー: {ErrorCode}, {Message}",
                ex.ErrorCode, ex.Message);
            return StatusCode(500, new ResizeImageResponse
            {
                Success = false,
                Message = ex.Message,
                ErrorCode = ex.ErrorCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "予期しないエラーが発生しました: {FilePath}", request.FilePath);
            return StatusCode(500, new ResizeImageResponse
            {
                Success = false,
                Message = "サーバーエラーが発生しました",
                ErrorCode = "INTERNAL_SERVER_ERROR"
            });
        }
    }
}

