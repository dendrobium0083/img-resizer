namespace ImgResizer.Application.DTOs;

/// <summary>
/// 画像リサイズレスポンスDTO
/// </summary>
public class ResizeImageResponse
{
    /// <summary>
    /// 処理が成功したかどうか
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 処理結果メッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 出力ファイルのパス（成功時のみ）
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// 使用されたリサイズモード
    /// </summary>
    public string? ResizeMode { get; set; }

    /// <summary>
    /// エラーコード（失敗時のみ）
    /// </summary>
    public string? ErrorCode { get; set; }
}
