namespace ImgResizer.Domain.Exceptions;
/// <summary>
/// 逕ｻ蜒丞・逅・ｸｭ縺ｫ繧ｨ繝ｩ繝ｼ縺檎匱逕溘＠縺溷ｴ蜷医・萓句､・
/// </summary>
public class ImageProcessingErrorException : ImageProcessingException
{
    public ImageProcessingErrorException(string message, Exception innerException)
        : base("IMAGE_PROCESSING_ERROR",
            $"逕ｻ蜒丞・逅・ｸｭ縺ｫ繧ｨ繝ｩ繝ｼ縺檎匱逕溘＠縺ｾ縺励◆: {message}",
            innerException)
    {
    }
}
