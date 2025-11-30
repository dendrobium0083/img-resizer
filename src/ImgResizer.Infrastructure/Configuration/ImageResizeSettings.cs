namespace ImgResizer.Infrastructure.Configuration;

/// <summary>
/// 画像リサイズ設定
/// </summary>
public class ImageResizeSettings
{
    public string InputDirectory { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
    public TargetSizeSettings TargetSize { get; set; } = new();
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
    public long MaxFileSize { get; set; } = 52428800; // 50MB
    public PaddingColorSettings? PaddingColor { get; set; }
    public ImageQualitySettings? ImageQuality { get; set; }
}

/// <summary>
/// ターゲットサイズ設定
/// </summary>
public class TargetSizeSettings
{
    public int Width { get; set; } = 512;
    public int Height { get; set; } = 512;
}

/// <summary>
/// パディング色設定
/// </summary>
public class PaddingColorSettings
{
    public int R { get; set; } = 0;
    public int G { get; set; } = 0;
    public int B { get; set; } = 0;
    public int A { get; set; } = 255;
}

/// <summary>
/// 画像品質設定
/// </summary>
public class ImageQualitySettings
{
    public int JpegQuality { get; set; } = 90;
    public int PngCompressionLevel { get; set; } = 6;
}

