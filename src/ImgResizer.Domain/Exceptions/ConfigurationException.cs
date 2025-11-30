namespace ImgResizer.Domain.Exceptions;

/// <summary>
/// 設定が不正な場合の例外
/// </summary>
public class ConfigurationException : ImageProcessingException
{
    public ConfigurationException(string message) 
        : base("INVALID_CONFIGURATION", $"設定が不正です: {message}")
    {
    }
}

