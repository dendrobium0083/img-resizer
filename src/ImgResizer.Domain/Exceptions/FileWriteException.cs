namespace ImgResizer.Domain.Exceptions;
/// <summary>
/// 繝輔ぃ繧､繝ｫ譖ｸ縺崎ｾｼ縺ｿ繧ｨ繝ｩ繝ｼ縺ｮ萓句､・
/// </summary>
public class FileWriteException : ImageProcessingException
{
    public FileWriteException(string filePath, Exception innerException)
        : base("FILE_WRITE_ERROR",
            $"繝輔ぃ繧､繝ｫ縺ｮ譖ｸ縺崎ｾｼ縺ｿ縺ｫ螟ｱ謨励＠縺ｾ縺励◆: {filePath}",
            innerException)
    {
    }
}
