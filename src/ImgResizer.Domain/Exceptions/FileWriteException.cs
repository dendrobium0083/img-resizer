namespace ImgResizer.Domain.Exceptions;

/// <summary>
/// ファイル書き込みエラーの例外
/// </summary>
public class FileWriteException : ImageProcessingException
{
    public FileWriteException(string filePath, Exception innerException) 
        : base("FILE_WRITE_ERROR", 
            $"ファイルの書き込みに失敗しました: {filePath}", 
            innerException)
    {
    }
}

