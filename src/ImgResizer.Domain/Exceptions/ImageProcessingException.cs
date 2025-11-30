namespace ImgResizer.Domain.Exceptions;

/// <summary>
/// 画像処理関連の例外の基底クラス
/// </summary>
public class ImageProcessingException : Exception
{
    public string ErrorCode { get; }

    public ImageProcessingException(string errorCode, string message) 
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public ImageProcessingException(string errorCode, string message, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

