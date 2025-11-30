namespace ImgResizer.Domain.Exceptions;

/// <summary>
/// バリデーションエラーの例外
/// </summary>
public class ValidationException : ImageProcessingException
{
    public ValidationException(string message) 
        : base("INVALID_REQUEST", message)
    {
    }
}

