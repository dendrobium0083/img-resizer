namespace ImgResizer.Application.DTOs;

/// <summary>
/// 画像リサイズリクエストDTO
/// </summary>
public class ResizeImageRequest
{
    /// <summary>
    /// リサイズ対象の画像ファイルパス
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// リサイズモード（"fit": アスペクト比を維持してパディング、"crop": 中央部分を切り出し）
    /// </summary>
    public string? ResizeMode { get; set; }
}
