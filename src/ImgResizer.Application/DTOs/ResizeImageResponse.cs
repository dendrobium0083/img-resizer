namespace ImgResizer.Application.DTOs;

/// <summary>
/// 画像リサイズレスポンスDTO
/// </summary>
public class ResizeImageResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? OutputPath { get; set; }
    public string? ResizeMode { get; set; }
    public string? ErrorCode { get; set; }
}

