namespace ImgResizer.Domain.Exceptions;

/// <summary>
/// 画像の読み込みに失敗した場合の例外
/// </summary>
public class ImageLoadException : ImageProcessingException
{
    public ImageLoadException(string filePath, Exception innerException) 
        : base("IMAGE_LOAD_ERROR", 
            $"画像の読み込みに失敗しました: {filePath}", 
            innerException)
    {
    }
}

