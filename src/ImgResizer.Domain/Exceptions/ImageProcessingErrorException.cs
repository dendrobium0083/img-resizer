namespace ImgResizer.Domain.Exceptions;

/// <summary>
/// 画像処理中にエラーが発生した場合の例外
/// </summary>
public class ImageProcessingErrorException : ImageProcessingException
{
    public ImageProcessingErrorException(string message, Exception innerException) 
        : base("IMAGE_PROCESSING_ERROR", 
            $"画像処理中にエラーが発生しました: {message}", 
            innerException)
    {
    }
}

