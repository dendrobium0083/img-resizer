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

    public FileSystemImageRepository(
        IOptions<ImageResizeSettings> settings,
        ILogger<FileSystemImageRepository> logger)
    {
        _settings = settings;
        _logger = logger;
    }

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

    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    public string GetOutputPath(string inputPath, string outputDirectory, string resizeMode)
    {
        var fileName = Path.GetFileNameWithoutExtension(inputPath);
        var extension = Path.GetExtension(inputPath);
        var suffix = resizeMode == "crop" ? "_512x512_crop" : "_512x512";
        var outputFileName = $"{fileName}{suffix}{extension}";
        return Path.Combine(outputDirectory, outputFileName);
    }
}

