namespace ImgResizer.Domain.Entities;

/// <summary>
/// 画像ファイルを表すエンティティ
/// </summary>
public class ImageFile
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}

