namespace ImgResizer.Domain.Exceptions;

/// <summary>
/// ファイルが見つからない場合の例外
/// </summary>
public class FileNotFoundException : ImageProcessingException
{
    public FileNotFoundException(string filePath) 
        : base("FILE_NOT_FOUND", $"ファイルが見つかりません: {filePath}")
    {
    }
}

