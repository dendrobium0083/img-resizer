using ImgResizer.Domain.Common;

namespace ImgResizer.Domain.Interfaces;

/// <summary>
/// 画像ファイルの読み書き操作を定義するインターフェース。
/// </summary>
public interface IImageRepository
{
    /// <summary>
    /// 画像ファイルを非同期で読み込みます。
    /// </summary>
    /// <param name="filePath">ファイルパス.</param>
    /// <returns>画像データ（バイト配列）を含むResult.</returns>
    Task<Result<byte[]>> ReadImageAsync(string filePath);

    /// <summary>
    /// 画像データを非同期で保存します。
    /// </summary>
    /// <param name="filePath">保存先のファイルパス.</param>
    /// <param name="imageData">画像データ（バイト配列）.</param>
    /// <returns>処理結果を表すResult.</returns>
    Task<Result> SaveImageAsync(string filePath, byte[] imageData);

    /// <summary>
    /// ファイルの存在を確認します。
    /// </summary>
    /// <param name="filePath">ファイルパス.</param>
    /// <returns>ファイルが存在する場合true.</returns>
    bool FileExists(string filePath);

    /// <summary>
    /// 出力ファイルパスを生成します。
    /// </summary>
    /// <param name="inputPath">入力ファイルパス.</param>
    /// <param name="outputDirectory">出力ディレクトリ.</param>
    /// <param name="resizeMode">変換方式（fit または crop）.</param>
    /// <returns>出力ファイルパス.</returns>
    string GetOutputPath(string inputPath, string outputDirectory, string resizeMode);
}
