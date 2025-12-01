namespace ImgResizer.Domain.Exceptions;
/// <summary>
/// 逕ｻ蜒丞・逅・未騾｣縺ｮ萓句､悶・蝓ｺ蠎輔け繝ｩ繧ｹ
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
