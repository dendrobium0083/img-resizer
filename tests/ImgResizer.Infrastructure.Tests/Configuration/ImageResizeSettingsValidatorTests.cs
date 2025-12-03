using FluentAssertions;
using ImgResizer.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ImgResizer.Infrastructure.Tests.Configuration;

/// <summary>
/// ImageResizeSettingsValidatorのテスト。
/// </summary>
public class ImageResizeSettingsValidatorTests
{
    private readonly ImageResizeSettingsValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageResizeSettingsValidatorTests"/> class.
    /// </summary>
    public ImageResizeSettingsValidatorTests()
    {
        _validator = new ImageResizeSettingsValidator();
    }

    /// <summary>
    /// 有効な設定の場合、検証が成功することを確認する。
    /// </summary>
    [Fact]
    public void Validate_ValidSettings_ReturnsSuccess()
    {
        // Arrange
        var settings = new ImageResizeSettings
        {
            OutputDirectory = "C:\\output",
            TargetSize = new TargetSizeSettings
            {
                Width = 512,
                Height = 512
            },
            AllowedExtensions = new[] { ".jpg", ".png" },
            MaxFileSize = 52428800,
            PaddingColor = new PaddingColorSettings
            {
                R = 0,
                G = 0,
                B = 0,
                A = 255
            },
            ImageQuality = new ImageQualitySettings
            {
                JpegQuality = 90,
                PngCompressionLevel = 6
            }
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Failed.Should().BeFalse();
    }

    /// <summary>
    /// OutputDirectoryが空の場合、検証が失敗することを確認する。
    /// </summary>
    [Fact]
    public void Validate_EmptyOutputDirectory_ReturnsFailure()
    {
        // Arrange
        var settings = new ImageResizeSettings
        {
            OutputDirectory = string.Empty,
            TargetSize = new TargetSizeSettings { Width = 512, Height = 512 },
            AllowedExtensions = new[] { ".jpg" },
            MaxFileSize = 52428800
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("OutputDirectoryは必須です");
    }

    /// <summary>
    /// TargetSizeがnullの場合、検証が失敗することを確認する。
    /// </summary>
    [Fact]
    public void Validate_NullTargetSize_ReturnsFailure()
    {
        // Arrange
        var settings = new ImageResizeSettings
        {
            OutputDirectory = "C:\\output",
            TargetSize = null!,
            AllowedExtensions = new[] { ".jpg" },
            MaxFileSize = 52428800
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("TargetSizeは必須です");
    }

    /// <summary>
    /// TargetSizeのWidthが0以下の場合、検証が失敗することを確認する。
    /// </summary>
    [Fact]
    public void Validate_InvalidTargetSizeWidth_ReturnsFailure()
    {
        // Arrange
        var settings = new ImageResizeSettings
        {
            OutputDirectory = "C:\\output",
            TargetSize = new TargetSizeSettings { Width = 0, Height = 512 },
            AllowedExtensions = new[] { ".jpg" },
            MaxFileSize = 52428800
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("TargetSize.Widthは正の値である必要があります");
    }

    /// <summary>
    /// AllowedExtensionsが空の場合、検証が失敗することを確認する。
    /// </summary>
    [Fact]
    public void Validate_EmptyAllowedExtensions_ReturnsFailure()
    {
        // Arrange
        var settings = new ImageResizeSettings
        {
            OutputDirectory = "C:\\output",
            TargetSize = new TargetSizeSettings { Width = 512, Height = 512 },
            AllowedExtensions = Array.Empty<string>(),
            MaxFileSize = 52428800
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("AllowedExtensionsは1つ以上指定する必要があります");
    }

    /// <summary>
    /// AllowedExtensionsに無効な拡張子が含まれている場合、検証が失敗することを確認する。
    /// </summary>
    [Fact]
    public void Validate_InvalidExtensionFormat_ReturnsFailure()
    {
        // Arrange
        var settings = new ImageResizeSettings
        {
            OutputDirectory = "C:\\output",
            TargetSize = new TargetSizeSettings { Width = 512, Height = 512 },
            AllowedExtensions = new[] { "jpg", ".png" }, // "jpg"は.で始まらない
            MaxFileSize = 52428800
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("AllowedExtensionsに無効な拡張子が含まれています");
    }

    /// <summary>
    /// MaxFileSizeが0以下の場合、検証が失敗することを確認する。
    /// </summary>
    [Fact]
    public void Validate_InvalidMaxFileSize_ReturnsFailure()
    {
        // Arrange
        var settings = new ImageResizeSettings
        {
            OutputDirectory = "C:\\output",
            TargetSize = new TargetSizeSettings { Width = 512, Height = 512 },
            AllowedExtensions = new[] { ".jpg" },
            MaxFileSize = 0
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("MaxFileSizeは正の値である必要があります");
    }

    /// <summary>
    /// PaddingColorの値が範囲外の場合、検証が失敗することを確認する。
    /// </summary>
    [Fact]
    public void Validate_InvalidPaddingColor_ReturnsFailure()
    {
        // Arrange
        var settings = new ImageResizeSettings
        {
            OutputDirectory = "C:\\output",
            TargetSize = new TargetSizeSettings { Width = 512, Height = 512 },
            AllowedExtensions = new[] { ".jpg" },
            MaxFileSize = 52428800,
            PaddingColor = new PaddingColorSettings
            {
                R = 256, // 範囲外
                G = 0,
                B = 0,
                A = 255
            }
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("PaddingColor.Rの値は0-255の範囲である必要があります");
    }

    /// <summary>
    /// ImageQualityのJpegQualityが範囲外の場合、検証が失敗することを確認する。
    /// </summary>
    [Fact]
    public void Validate_InvalidJpegQuality_ReturnsFailure()
    {
        // Arrange
        var settings = new ImageResizeSettings
        {
            OutputDirectory = "C:\\output",
            TargetSize = new TargetSizeSettings { Width = 512, Height = 512 },
            AllowedExtensions = new[] { ".jpg" },
            MaxFileSize = 52428800,
            ImageQuality = new ImageQualitySettings
            {
                JpegQuality = 101, // 範囲外
                PngCompressionLevel = 6
            }
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("ImageQuality.JpegQualityは1-100の範囲である必要があります");
    }

    /// <summary>
    /// ImageQualityのPngCompressionLevelが範囲外の場合、検証が失敗することを確認する。
    /// </summary>
    [Fact]
    public void Validate_InvalidPngCompressionLevel_ReturnsFailure()
    {
        // Arrange
        var settings = new ImageResizeSettings
        {
            OutputDirectory = "C:\\output",
            TargetSize = new TargetSizeSettings { Width = 512, Height = 512 },
            AllowedExtensions = new[] { ".jpg" },
            MaxFileSize = 52428800,
            ImageQuality = new ImageQualitySettings
            {
                JpegQuality = 90,
                PngCompressionLevel = 10 // 範囲外
            }
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("ImageQuality.PngCompressionLevelは0-9の範囲である必要があります");
    }
}

