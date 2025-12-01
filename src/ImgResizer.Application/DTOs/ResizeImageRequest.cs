namespace ImgResizer.Application.DTOs;

/// <summary>
/// 画像リサイズリクエストDTO
/// </summary>
public class ResizeImageRequest
{
    public string FilePath { get; set; } = string.Empty;

    public string? ResizeMode { get; set; }
}
