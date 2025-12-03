using Microsoft.Extensions.Options;

namespace ImgResizer.Infrastructure.Configuration;

/// <summary>
/// ImageResizeSettingsの設定値検証クラス
/// </summary>
public class ImageResizeSettingsValidator : IValidateOptions<ImageResizeSettings>
{
    /// <summary>
    /// 設定値を検証する
    /// </summary>
    /// <param name="name">設定名（通常はnull）</param>
    /// <param name="options">検証対象の設定オブジェクト</param>
    /// <returns>検証結果</returns>
    public ValidateOptionsResult Validate(string? name, ImageResizeSettings options)
    {
        var errors = new List<string>();

        // OutputDirectoryの検証
        if (string.IsNullOrWhiteSpace(options.OutputDirectory))
        {
            errors.Add("OutputDirectoryは必須です");
        }

        // TargetSizeの検証
        if (options.TargetSize == null)
        {
            errors.Add("TargetSizeは必須です");
        }
        else
        {
            if (options.TargetSize.Width <= 0)
            {
                errors.Add("TargetSize.Widthは正の値である必要があります");
            }

            if (options.TargetSize.Height <= 0)
            {
                errors.Add("TargetSize.Heightは正の値である必要があります");
            }
        }

        // AllowedExtensionsの検証
        if (options.AllowedExtensions == null || options.AllowedExtensions.Length == 0)
        {
            errors.Add("AllowedExtensionsは1つ以上指定する必要があります");
        }
        else
        {
            // 拡張子が正しい形式かチェック（.で始まるか）
            var invalidExtensions = options.AllowedExtensions
                .Where(ext => string.IsNullOrWhiteSpace(ext) || !ext.StartsWith('.'))
                .ToList();

            if (invalidExtensions.Any())
            {
                errors.Add($"AllowedExtensionsに無効な拡張子が含まれています: {string.Join(", ", invalidExtensions)}");
            }
        }

        // MaxFileSizeの検証
        if (options.MaxFileSize <= 0)
        {
            errors.Add("MaxFileSizeは正の値である必要があります");
        }

        // PaddingColorの検証（オプション）
        if (options.PaddingColor != null)
        {
            if (options.PaddingColor.R < 0 || options.PaddingColor.R > 255)
            {
                errors.Add("PaddingColor.Rの値は0-255の範囲である必要があります");
            }

            if (options.PaddingColor.G < 0 || options.PaddingColor.G > 255)
            {
                errors.Add("PaddingColor.Gの値は0-255の範囲である必要があります");
            }

            if (options.PaddingColor.B < 0 || options.PaddingColor.B > 255)
            {
                errors.Add("PaddingColor.Bの値は0-255の範囲である必要があります");
            }

            if (options.PaddingColor.A < 0 || options.PaddingColor.A > 255)
            {
                errors.Add("PaddingColor.Aの値は0-255の範囲である必要があります");
            }
        }

        // ImageQualityの検証（オプション）
        if (options.ImageQuality != null)
        {
            if (options.ImageQuality.JpegQuality < 1 || options.ImageQuality.JpegQuality > 100)
            {
                errors.Add("ImageQuality.JpegQualityは1-100の範囲である必要があります");
            }

            if (options.ImageQuality.PngCompressionLevel < 0 || options.ImageQuality.PngCompressionLevel > 9)
            {
                errors.Add("ImageQuality.PngCompressionLevelは0-9の範囲である必要があります");
            }
        }

        if (errors.Any())
        {
            return ValidateOptionsResult.Fail(errors);
        }

        return ValidateOptionsResult.Success;
    }
}


