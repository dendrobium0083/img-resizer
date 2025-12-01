namespace ImgResizer.Domain.Exceptions;
/// <summary>
/// 繝輔ぃ繧､繝ｫ繧ｵ繧､繧ｺ縺悟､ｧ縺阪☆縺弱ｋ蝣ｴ蜷医・萓句､・
/// </summary>
public class FileTooLargeException : ImageProcessingException
{
    public FileTooLargeException(long fileSize, long maxSize)
        : base("FILE_TOO_LARGE",
            $"繝輔ぃ繧､繝ｫ繧ｵ繧､繧ｺ縺悟､ｧ縺阪☆縺弱∪縺吶ら樟蝨ｨ: {FormatFileSize(fileSize)}縲∵怙螟ｧ: {FormatFileSize(maxSize)}")
    {
    }
    private static string FormatFileSize(long bytes)
    {
        // 繝輔ぃ繧､繝ｫ繧ｵ繧､繧ｺ繧定ｪｭ縺ｿ繧・☆縺・ｽ｢蠑上↓螟画鋤・井ｾ・ "50MB"・・
        return $"{bytes / 1024 / 1024}MB";
    }
}
