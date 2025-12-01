using ImgResizer.Domain.Common;

namespace ImgResizer.Domain.Interfaces;

/// <summary>
/// 画像リサイズ処理を定義するインターフェース
/// </summary>
public interface IImageResizeService
{
    /// <summary>
    /// 画像を正方形にリサイズする（非同期）
    /// </summary>
    /// <param name="imageData">元画像データ（バイト配列）</param>
    /// <param name="size">ターゲットサイズ（512）</param>
    /// <param name="resizeMode">変換方式（fit または crop）</param>
    /// <param name="extension">画像の拡張子</param>
    /// <returns>リサイズ後の画像データ（バイト配列）を含むResult</returns>
    Task<Result<byte[]>> ResizeToSquareAsync(byte[] imageData, int size, string resizeMode, string extension);

    /// <summary>
    /// サポートされている画像形式か判定する
    /// </summary>
    /// <param name="filePath">ファイルパス</param>
    /// <returns>サポートされている場合true</returns>
    bool IsSupportedFormat(string filePath);
}

