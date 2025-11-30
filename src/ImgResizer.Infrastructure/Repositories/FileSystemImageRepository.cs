using ImgResizer.Domain.Exceptions;
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

    public async Task<byte[]> ReadImageAsync(string filePath)
    {
        try
        {
            _logger.LogDebug("画像ファイルを読み込み中: {FilePath}", filePath);
            return await File.ReadAllBytesAsync(filePath);
        }
        catch (System.IO.FileNotFoundException)
        {
            throw new Domain.Exceptions.FileNotFoundException(filePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "ファイル読み込み権限エラー: {FilePath}", filePath);
            throw new FileReadException(filePath, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ファイル読み込みエラー: {FilePath}", filePath);
            throw new FileReadException(filePath, ex);
        }
    }

    public async Task SaveImageAsync(string filePath, byte[] imageData)
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
            _logger.LogDebug("画像ファイルを保存しました: {FilePath}", filePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "ファイル書き込み権限エラー: {FilePath}", filePath);
            throw new FileWriteException(filePath, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ファイル書き込みエラー: {FilePath}", filePath);
            throw new FileWriteException(filePath, ex);
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

