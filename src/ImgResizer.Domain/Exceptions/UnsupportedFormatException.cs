namespace ImgResizer.Domain.Exceptions;

/// <summary>
/// サポートされていない画像形式の場合の例外
/// </summary>
public class UnsupportedFormatException : ImageProcessingException
{
    public UnsupportedFormatException(string extension) 
        : base("INVALID_FILE_FORMAT", $"サポートされていない画像形式です: {extension}")
    {
    }
}

