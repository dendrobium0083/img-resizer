using ImgResizer.Domain.Common;
using ImgResizer.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ImgResizer.Infrastructure.Configuration;

namespace ImgResizer.Infrastructure.Repositories;

/// <summary>
/// ファイルシステムへの画像アクセス実装
/// </summary>
public class FileSystemImageRepository : IImageRepository
{
    private readonly IOptions<ImageResizeSettings> _settings;
    private readonly ILogger<FileSystemImageRepository> _logger;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="settings">画像リサイズ設定</param>
    /// <param name="logger">ロガー</param>
    public FileSystemImageRepository(
        IOptions<ImageResizeSettings> settings,
        ILogger<FileSystemImageRepository> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// 画像ファイルを読み込む
    /// </summary>
    /// <param name="filePath">ファイルパス</param>
    /// <returns>画像データ（バイト配列）を含むResult</returns>
    public async Task<Result<byte[]>> ReadImageAsync(string filePath)
    {
        try
        {
            _logger.LogDebug("画像ファイルを読み込み中: {FilePath}", filePath);
            
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("ファイルが見つかりません: {FilePath}", filePath);
                return Result.Failure<byte[]>(
                    "FILE_NOT_FOUND",
                    $"ファイルが見つかりません: {filePath}");
            }

            var data = await File.ReadAllBytesAsync(filePath);
            _logger.LogDebug("画像ファイルを読み込みました: {FilePath}, Size={Size} bytes", filePath, data.Length);
            return Result.Success(data);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "ファイル読み込み権限エラー: {FilePath}", filePath);
            return Result.Failure<byte[]>(
                "FILE_READ_ERROR",
                $"ファイルへのアクセス権限がありません: {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ファイル読み込みエラー: {FilePath}", filePath);
            return Result.Failure<byte[]>(
                "FILE_READ_ERROR",
                $"ファイルの読み込みに失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// 画像ファイルを保存する
    /// </summary>
    /// <param name="filePath">保存先ファイルパス</param>
    /// <param name="imageData">画像データ（バイト配列）</param>
    /// <returns>処理結果を含むResult</returns>
    public async Task<Result> SaveImageAsync(string filePath, byte[] imageData)
    {
        try
        {
            _logger.LogDebug("画像ファイルを保存中: {FilePath}", filePath);
            
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogDebug("出力ディレクトリを作成しました: {Directory}", directory);
            }

            await File.WriteAllBytesAsync(filePath, imageData);
            _logger.LogDebug("画像ファイルを保存しました: {FilePath}, Size={Size} bytes", filePath, imageData.Length);
            
            return Result.Success();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "ファイル書き込み権限エラー: {FilePath}", filePath);
            return Result.Failure(
                "FILE_WRITE_ERROR",
                $"ファイルへの書き込み権限がありません: {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ファイル書き込みエラー: {FilePath}", filePath);
            return Result.Failure(
                "FILE_WRITE_ERROR",
                $"ファイルの保存に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// ファイルが存在するか確認する
    /// </summary>
    /// <param name="filePath">ファイルパス</param>
    /// <returns>存在する場合true</returns>
    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    /// <summary>
    /// 出力ファイルパスを生成する
    /// </summary>
    /// <param name="inputPath">入力ファイルパス</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <param name="resizeMode">リサイズモード</param>
    /// <returns>出力ファイルパス</returns>
    public string GetOutputPath(string inputPath, string outputDirectory, string resizeMode)
    {
        var fileName = Path.GetFileNameWithoutExtension(inputPath);
        var extension = Path.GetExtension(inputPath);
        var suffix = resizeMode == "crop" ? "_512x512_crop" : "_512x512";
        var outputFileName = $"{fileName}{suffix}{extension}";
        return Path.Combine(outputDirectory, outputFileName);
    }
}

