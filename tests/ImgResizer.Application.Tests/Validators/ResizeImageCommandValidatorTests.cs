using FluentAssertions;
using ImgResizer.Application.Commands;
using ImgResizer.Application.Validators;
using ImgResizer.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ImgResizer.Application.Tests.Validators;

/// <summary>
/// ResizeImageCommandValidatorのテスト。
/// </summary>
public class ResizeImageCommandValidatorTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _testFilePath;
    private readonly ResizeImageCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResizeImageCommandValidatorTests"/> class.
    /// </summary>
    public ResizeImageCommandValidatorTests()
    {
        // テスト用の一時ディレクトリとファイルを作成
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _testFilePath = Path.Combine(_testDirectory, "test.jpg");

        // テスト用の画像ファイルを作成（最小限のJPEGデータ）
        var testImageData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
        File.WriteAllBytes(_testFilePath, testImageData);

        // バリデーターの設定
        var settings = new ImageResizeSettings
        {
            AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" },
            MaxFileSize = 10 * 1024 * 1024 // 10MB
        };
        var options = Options.Create(settings);
        _validator = new ResizeImageCommandValidator(options);
    }

    /// <summary>
    /// テスト用リソースをクリーンアップします。
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public async Task Validate_ValidCommand_ReturnsValid()
    {
        // Arrange
        var command = new ResizeImageCommand(_testFilePath, "fit");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ValidCommand_WithFitMode_ReturnsValid()
    {
        // Arrange
        var command = new ResizeImageCommand(_testFilePath, "fit");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ValidCommand_WithCropMode_ReturnsValid()
    {
        // Arrange
        var command = new ResizeImageCommand(_testFilePath, "crop");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ValidCommand_WithoutResizeMode_ReturnsValid()
    {
        // Arrange
        var command = new ResizeImageCommand(_testFilePath, null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyFilePath_ReturnsInvalid()
    {
        // Arrange
        var command = new ResizeImageCommand(string.Empty, "fit");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "VALIDATION_ERROR");
        result.Errors.Should().Contain(e => e.ErrorMessage == "ファイルパスが指定されていません");
    }

    [Fact]
    public async Task Validate_FilePathWithPathTraversal_ReturnsInvalid()
    {
        // Arrange
        var command = new ResizeImageCommand("../test.jpg", "fit");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "VALIDATION_ERROR");
        result.Errors.Should().Contain(e => e.ErrorMessage == "無効なファイルパスです");
    }

    [Fact]
    public async Task Validate_FilePathWithTilde_ReturnsInvalid()
    {
        // Arrange
        var command = new ResizeImageCommand("~/test.jpg", "fit");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "VALIDATION_ERROR");
        result.Errors.Should().Contain(e => e.ErrorMessage == "無効なファイルパスです");
    }

    [Fact]
    public async Task Validate_InvalidResizeMode_ReturnsInvalid()
    {
        // Arrange
        var command = new ResizeImageCommand(_testFilePath, "invalid");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "VALIDATION_ERROR");
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("無効な変換方式です"));
    }

    [Fact]
    public async Task Validate_FileNotFound_ReturnsInvalid()
    {
        // Arrange
        var command = new ResizeImageCommand("nonexistent.jpg", "fit");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "FILE_NOT_FOUND");
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("ファイルが見つかりません"));
    }

    [Fact]
    public async Task Validate_UnsupportedFormat_ReturnsInvalid()
    {
        // Arrange
        var unsupportedFilePath = Path.Combine(_testDirectory, "test.gif");
        File.WriteAllBytes(unsupportedFilePath, new byte[] { 1, 2, 3 });
        var command = new ResizeImageCommand(unsupportedFilePath, "fit");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "UNSUPPORTED_FORMAT");
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("サポートされていない画像形式です"));
    }

    [Fact]
    public async Task Validate_FileTooLarge_ReturnsInvalid()
    {
        // Arrange
        var largeFilePath = Path.Combine(_testDirectory, "large.jpg");
        var largeFileData = new byte[11 * 1024 * 1024]; // 11MB（上限10MBを超える）
        File.WriteAllBytes(largeFilePath, largeFileData);

        var settings = new ImageResizeSettings
        {
            AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" },
            MaxFileSize = 10 * 1024 * 1024 // 10MB
        };
        var options = Options.Create(settings);
        var validator = new ResizeImageCommandValidator(options);

        var command = new ResizeImageCommand(largeFilePath, "fit");

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorCode == "FILE_TOO_LARGE");
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("ファイルサイズが大きすぎます"));
    }

    [Fact]
    public async Task Validate_ValidPngFile_ReturnsValid()
    {
        // Arrange
        var pngFilePath = Path.Combine(_testDirectory, "test.png");
        var pngData = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        File.WriteAllBytes(pngFilePath, pngData);
        var command = new ResizeImageCommand(pngFilePath, "fit");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ValidBmpFile_ReturnsValid()
    {
        // Arrange
        var bmpFilePath = Path.Combine(_testDirectory, "test.bmp");
        var bmpData = new byte[] { 0x42, 0x4D, 0x36, 0x00, 0x00, 0x00 };
        File.WriteAllBytes(bmpFilePath, bmpData);
        var command = new ResizeImageCommand(bmpFilePath, "fit");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

