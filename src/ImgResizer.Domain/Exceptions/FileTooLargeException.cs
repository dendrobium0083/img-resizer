namespace ImgResizer.Domain.Exceptions;

/// <summary>
/// ファイルサイズが大きすぎる場合の例外
/// </summary>
public class FileTooLargeException : ImageProcessingException
{
    public FileTooLargeException(long fileSize, long maxSize) 
        : base("FILE_TOO_LARGE", 
            $"ファイルサイズが大きすぎます。現在: {FormatFileSize(fileSize)}、最大: {FormatFileSize(maxSize)}")
    {
    }

    private static string FormatFileSize(long bytes)
    {
        // ファイルサイズを読みやすい形式に変換（例: "50MB"）
        return $"{bytes / 1024 / 1024}MB";
    }
}

