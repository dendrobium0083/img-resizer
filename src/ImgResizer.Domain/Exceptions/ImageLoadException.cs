namespace ImgResizer.Domain.Exceptions;
/// <summary>
/// 逕ｻ蜒上・隱ｭ縺ｿ霎ｼ縺ｿ縺ｫ螟ｱ謨励＠縺溷ｴ蜷医・萓句､・
/// </summary>
public class ImageLoadException : ImageProcessingException
{
    public ImageLoadException(string filePath, Exception innerException)
        : base("IMAGE_LOAD_ERROR",
            $"逕ｻ蜒上・隱ｭ縺ｿ霎ｼ縺ｿ縺ｫ螟ｱ謨励＠縺ｾ縺励◆: {filePath}",
            innerException)
    {
    }
}
