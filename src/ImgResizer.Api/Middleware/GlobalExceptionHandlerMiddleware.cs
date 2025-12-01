using ImgResizer.Application.DTOs;
using ImgResizer.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace ImgResizer.Api.Middleware;

/// <summary>
/// グローバル例外ハンドラーミドルウェア
/// 予期しない例外を一元的に処理する
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="next">次のミドルウェア</param>
    /// <param name="logger">ロガー</param>
    /// <param name="environment">ホスト環境</param>
    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// ミドルウェアの処理を実行する
    /// </summary>
    /// <param name="context">HTTPコンテキスト</param>
    /// <returns>非同期タスク</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
#pragma warning disable CA1031 // Do not catch general exception types - グローバル例外ハンドラーでは必要
        catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            _logger.LogError(ex, "予期しないエラーが発生しました");
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// 例外を処理してエラーレスポンスを返す
    /// </summary>
    /// <param name="context">HTTPコンテキスト</param>
    /// <param name="exception">発生した例外</param>
    /// <returns>非同期タスク</returns>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // HTTPステータスコードとエラーレスポンスを決定
        var (statusCode, errorResponse) = exception switch
        {
            Domain.Exceptions.FileNotFoundException ex => 
                (HttpStatusCode.NotFound, 
                 CreateErrorResponse(ex.ErrorCode, ex.Message)),
                 
            ValidationException ex => 
                (HttpStatusCode.BadRequest, 
                 CreateErrorResponse(ex.ErrorCode, ex.Message)),
                 
            UnsupportedFormatException ex => 
                (HttpStatusCode.BadRequest, 
                 CreateErrorResponse(ex.ErrorCode, ex.Message)),
                 
            FileTooLargeException ex => 
                (HttpStatusCode.BadRequest, 
                 CreateErrorResponse(ex.ErrorCode, ex.Message)),

            FileReadException ex => 
                (HttpStatusCode.InternalServerError, 
                 CreateErrorResponse(ex.ErrorCode, ex.Message)),

            FileWriteException ex => 
                (HttpStatusCode.InternalServerError, 
                 CreateErrorResponse(ex.ErrorCode, ex.Message)),

            ImageLoadException ex => 
                (HttpStatusCode.BadRequest, 
                 CreateErrorResponse(ex.ErrorCode, ex.Message)),

            ImageProcessingErrorException ex => 
                (HttpStatusCode.InternalServerError, 
                 CreateErrorResponse(ex.ErrorCode, ex.Message)),
                 
            ImageProcessingException ex => 
                (HttpStatusCode.InternalServerError, 
                 CreateErrorResponse(ex.ErrorCode, ex.Message)),
                 
            _ => 
                (HttpStatusCode.InternalServerError, 
                 CreateErrorResponse("INTERNAL_SERVER_ERROR", 
                     _environment.IsDevelopment() 
                         ? $"予期しないエラーが発生しました: {exception.Message}" 
                         : "予期しないエラーが発生しました"))
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        });

        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// エラーレスポンスを作成する
    /// </summary>
    /// <param name="errorCode">エラーコード</param>
    /// <param name="message">エラーメッセージ</param>
    /// <returns>エラーレスポンス</returns>
    private static ResizeImageResponse CreateErrorResponse(string errorCode, string message)
    {
        return new ResizeImageResponse
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message
        };
    }
}
