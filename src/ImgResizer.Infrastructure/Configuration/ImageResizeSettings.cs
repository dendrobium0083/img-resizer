namespace ImgResizer.Infrastructure.Configuration;

/// <summary>
/// 画像リサイズ設定
/// </summary>
public class ImageResizeSettings
{
    /// <summary>
    /// 入力ディレクトリパス
    /// </summary>
    public string InputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 出力ディレクトリパス
    /// </summary>
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// ターゲットサイズ設定
    /// </summary>
    public TargetSizeSettings TargetSize { get; set; } = new();

    /// <summary>
    /// 許可される画像ファイル拡張子のリスト
    /// </summary>
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 最大ファイルサイズ（バイト）
    /// </summary>
    public long MaxFileSize { get; set; } = 52428800; // 50MB

    /// <summary>
    /// パディング色設定（オプション）
    /// </summary>
    public PaddingColorSettings? PaddingColor { get; set; }

    /// <summary>
    /// 画像品質設定（オプション）
    /// </summary>
    public ImageQualitySettings? ImageQuality { get; set; }
}

/// <summary>
/// ターゲットサイズ設定
/// </summary>
public class TargetSizeSettings
{
    /// <summary>
    /// ターゲット幅（ピクセル）
    /// </summary>
    public int Width { get; set; } = 512;

    /// <summary>
    /// ターゲット高さ（ピクセル）
    /// </summary>
    public int Height { get; set; } = 512;
}

/// <summary>
/// パディング色設定
/// </summary>
public class PaddingColorSettings
{
    /// <summary>
    /// 赤成分（0-255）
    /// </summary>
    public int R { get; set; } = 0;

    /// <summary>
    /// 緑成分（0-255）
    /// </summary>
    public int G { get; set; } = 0;

    /// <summary>
    /// 青成分（0-255）
    /// </summary>
    public int B { get; set; } = 0;

    /// <summary>
    /// アルファ成分（0-255）
    /// </summary>
    public int A { get; set; } = 255;
}

/// <summary>
/// 画像品質設定
/// </summary>
public class ImageQualitySettings
{
    /// <summary>
    /// JPEG品質（1-100、高いほど高品質）
    /// </summary>
    public int JpegQuality { get; set; } = 90;

    /// <summary>
    /// PNG圧縮レベル（1-9、高いほど圧縮率が高い）
    /// </summary>
    public int PngCompressionLevel { get; set; } = 6;
}
