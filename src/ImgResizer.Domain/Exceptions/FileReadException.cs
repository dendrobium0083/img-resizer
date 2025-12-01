namespace ImgResizer.Domain.Exceptions;
/// <summary>
/// 繝輔ぃ繧､繝ｫ隱ｭ縺ｿ霎ｼ縺ｿ繧ｨ繝ｩ繝ｼ縺ｮ萓句､・
/// </summary>
public class FileReadException : ImageProcessingException
{
    public FileReadException(string filePath, Exception innerException)
        : base("FILE_READ_ERROR",
            $"繝輔ぃ繧､繝ｫ縺ｮ隱ｭ縺ｿ霎ｼ縺ｿ縺ｫ螟ｱ謨励＠縺ｾ縺励◆: {filePath}",
            innerException)
    {
    }
}
