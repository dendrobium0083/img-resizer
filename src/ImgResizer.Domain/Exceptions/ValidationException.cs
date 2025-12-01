namespace ImgResizer.Domain.Exceptions;
/// <summary>
/// 繝舌Μ繝・・繧ｷ繝ｧ繝ｳ繧ｨ繝ｩ繝ｼ縺ｮ萓句､・
/// </summary>
public class ValidationException : ImageProcessingException
{
    public ValidationException(string message)
        : base("INVALID_REQUEST", message)
    {
    }
}
