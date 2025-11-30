namespace ImgResizer.Domain.Exceptions;

/// <summary>
/// ファイル読み込みエラーの例外
/// </summary>
public class FileReadException : ImageProcessingException
{
    public FileReadException(string filePath, Exception innerException) 
        : base("FILE_READ_ERROR", 
            $"ファイルの読み込みに失敗しました: {filePath}", 
            innerException)
    {
    }
}

