namespace ImgResizer.Domain.Exceptions;
/// <summary>
/// 繝輔ぃ繧､繝ｫ縺瑚ｦ九▽縺九ｉ縺ｪ縺・ｴ蜷医・萓句､・
/// </summary>
public class FileNotFoundException : ImageProcessingException
{
    public FileNotFoundException(string filePath)
        : base("FILE_NOT_FOUND", $"繝輔ぃ繧､繝ｫ縺瑚ｦ九▽縺九ｊ縺ｾ縺帙ｓ: {filePath}")
    {
    }
}
