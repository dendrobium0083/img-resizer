namespace ImgResizer.Domain.Exceptions;
/// <summary>
/// 險ｭ螳壹′荳肴ｭ｣縺ｪ蝣ｴ蜷医・萓句､・
/// </summary>
public class ConfigurationException : ImageProcessingException
{
    public ConfigurationException(string message)
        : base("INVALID_CONFIGURATION", $"險ｭ螳壹′荳肴ｭ｣縺ｧ縺・ {message}")
    {
    }
}
