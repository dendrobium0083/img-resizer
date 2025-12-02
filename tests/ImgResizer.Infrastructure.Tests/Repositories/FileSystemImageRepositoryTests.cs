using FluentAssertions;
using ImgResizer.Domain.Common;
using ImgResizer.Infrastructure.Configuration;
using ImgResizer.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ImgResizer.Infrastructure.Tests.Repositories;

/// <summary>
/// FileSystemImageRepositoryのテスト。
/// </summary>
public class FileSystemImageRepositoryTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _testFilePath;
    private readonly FileSystemImageRepository _repository;
    private readonly Mock<ILogger<FileSystemImageRepository>> _mockLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemImageRepositoryTests"/> class.
    /// </summary>
    public FileSystemImageRepositoryTests()
    {
        // テスト用の一時ディレクトリとファイルを作成
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _testFilePath = Path.Combine(_testDirectory, "test.jpg");

        // テスト用の画像ファイルを作成（最小限のJPEGデータ）
        var testImageData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
        File.WriteAllBytes(_testFilePath, testImageData);

        // リポジトリの設定
        var settings = new ImageResizeSettings
        {
            OutputDirectory = Path.Combine(_testDirectory, "output")
        };
        var options = Options.Create(settings);
        _mockLogger = new Mock<ILogger<FileSystemImageRepository>>();
        _repository = new FileSystemImageRepository(options, _mockLogger.Object);
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
    public async Task ReadImageAsync_ExistingFile_ReturnsSuccess()
    {
        // Act
        var result = await _repository.ReadImageAsync(_testFilePath);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ReadImageAsync_NonExistentFile_ReturnsFailure()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.jpg");

        // Act
        var result = await _repository.ReadImageAsync(nonExistentPath);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("FILE_NOT_FOUND");
        result.ErrorMessage.Should().Contain("ファイルが見つかりません");
    }

    [Fact]
    public async Task SaveImageAsync_ValidData_SavesFile()
    {
        // Arrange
        var outputPath = Path.Combine(_testDirectory, "output", "saved.jpg");
        var testData = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var result = await _repository.SaveImageAsync(outputPath, testData);

        // Assert
        result.IsSuccess.Should().BeTrue();
        File.Exists(outputPath).Should().BeTrue();
        var savedData = await File.ReadAllBytesAsync(outputPath);
        savedData.Should().BeEquivalentTo(testData);
    }

    [Fact]
    public async Task SaveImageAsync_CreatesOutputDirectory()
    {
        // Arrange
        var outputDir = Path.Combine(_testDirectory, "new-output");
        var outputPath = Path.Combine(outputDir, "saved.jpg");
        var testData = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var result = await _repository.SaveImageAsync(outputPath, testData);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Directory.Exists(outputDir).Should().BeTrue();
        File.Exists(outputPath).Should().BeTrue();
    }

    [Fact]
    public void GetOutputPath_WithFitMode_GeneratesCorrectPath()
    {
        // Arrange
        var inputPath = Path.Combine(_testDirectory, "input", "test.jpg");
        var outputDirectory = Path.Combine(_testDirectory, "output");
        const string resizeMode = "fit";

        // Act
        var outputPath = _repository.GetOutputPath(inputPath, outputDirectory, resizeMode);

        // Assert
        outputPath.Should().Be(Path.Combine(outputDirectory, "test_512x512.jpg"));
    }

    [Fact]
    public void GetOutputPath_WithCropMode_GeneratesCorrectPath()
    {
        // Arrange
        var inputPath = Path.Combine(_testDirectory, "input", "test.jpg");
        var outputDirectory = Path.Combine(_testDirectory, "output");
        const string resizeMode = "crop";

        // Act
        var outputPath = _repository.GetOutputPath(inputPath, outputDirectory, resizeMode);

        // Assert
        outputPath.Should().Be(Path.Combine(outputDirectory, "test_512x512_crop.jpg"));
    }

    [Fact]
    public void GetOutputPath_WithPngExtension_GeneratesCorrectPath()
    {
        // Arrange
        var inputPath = Path.Combine(_testDirectory, "input", "test.png");
        var outputDirectory = Path.Combine(_testDirectory, "output");
        const string resizeMode = "fit";

        // Act
        var outputPath = _repository.GetOutputPath(inputPath, outputDirectory, resizeMode);

        // Assert
        outputPath.Should().Be(Path.Combine(outputDirectory, "test_512x512.png"));
    }

    [Fact]
    public void FileExists_ExistingFile_ReturnsTrue()
    {
        // Act
        var result = _repository.FileExists(_testFilePath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FileExists_NonExistentFile_ReturnsFalse()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.jpg");

        // Act
        var result = _repository.FileExists(nonExistentPath);

        // Assert
        result.Should().BeFalse();
    }
}

