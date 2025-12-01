namespace ImgResizer.Domain.Exceptions;
/// <summary>
/// 繧ｵ繝昴・繝医＆繧後※縺・↑縺・判蜒丞ｽ｢蠑上・蝣ｴ蜷医・萓句､・
/// </summary>
public class UnsupportedFormatException : ImageProcessingException
{
    public UnsupportedFormatException(string extension)
        : base("INVALID_FILE_FORMAT", $"繧ｵ繝昴・繝医＆繧後※縺・↑縺・判蜒丞ｽ｢蠑上〒縺・ {extension}")
    {
    }
}
