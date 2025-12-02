using FluentAssertions;
using ImgResizer.Domain.Common;
using ImgResizer.Infrastructure.Configuration;
using ImgResizer.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace ImgResizer.Infrastructure.Tests.Services;

/// <summary>
/// ImageResizeServiceのテスト。
/// </summary>
public class ImageResizeServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ImageResizeService _service;
    private readonly Mock<ILogger<ImageResizeService>> _mockLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageResizeServiceTests"/> class.
    /// </summary>
    public ImageResizeServiceTests()
    {
        // テスト用の一時ディレクトリを作成
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);

        // サービスの設定
        var settings = new ImageResizeSettings
        {
            AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" }
        };
        var options = Options.Create(settings);
        _mockLogger = new Mock<ILogger<ImageResizeService>>();
        _service = new ImageResizeService(_mockLogger.Object, options);
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

    /// <summary>
    /// テスト用の画像データを作成します。
    /// </summary>
    /// <param name="width">幅。</param>
    /// <param name="height">高さ。</param>
    /// <param name="encoder">画像エンコーダー。</param>
    /// <returns>画像データ（バイト配列）。</returns>
    private byte[] CreateTestImage(int width, int height, IImageEncoder encoder)
    {
        using var image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height);
        
        // 背景を赤に設定
        image.Mutate(ctx => ctx.BackgroundColor(SixLabors.ImageSharp.Color.Red));

        // 中央に青い円を描画（簡易版：中央付近のピクセルを青に設定）
        var centerX = width / 2;
        var centerY = height / 2;
        var radius = Math.Min(width, height) / 4;
        for (int x = Math.Max(0, centerX - radius); x < Math.Min(width, centerX + radius); x++)
        {
            for (int y = Math.Max(0, centerY - radius); y < Math.Min(height, centerY + radius); y++)
            {
                var dx = x - centerX;
                var dy = y - centerY;
                if (dx * dx + dy * dy <= radius * radius)
                {
                    image[x, y] = SixLabors.ImageSharp.Color.Blue;
                }
            }
        }

        using var stream = new MemoryStream();
        image.Save(stream, encoder);
        return stream.ToArray();
    }

    [Fact]
    public async Task ResizeToSquareAsync_ValidImage_WithFitMode_ReturnsSuccess()
    {
        // Arrange
        var originalImageData = CreateTestImage(800, 600, new JpegEncoder());
        const int targetSize = 512;
        const string resizeMode = "fit";
        const string extension = ".jpg";

        // Act
        var result = await _service.ResizeToSquareAsync(originalImageData, targetSize, resizeMode, extension);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ResizeToSquareAsync_ValidImage_WithCropMode_ReturnsSuccess()
    {
        // Arrange
        var originalImageData = CreateTestImage(800, 600, new JpegEncoder());
        const int targetSize = 512;
        const string resizeMode = "crop";
        const string extension = ".jpg";

        // Act
        var result = await _service.ResizeToSquareAsync(originalImageData, targetSize, resizeMode, extension);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ResizeToSquareAsync_OutputSize_Is512x512()
    {
        // Arrange
        var originalImageData = CreateTestImage(800, 600, new JpegEncoder());
        const int targetSize = 512;
        const string resizeMode = "fit";
        const string extension = ".jpg";

        // Act
        var result = await _service.ResizeToSquareAsync(originalImageData, targetSize, resizeMode, extension);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // 出力画像のサイズを確認
        using var outputImage = Image.Load(result.Value);
        outputImage.Width.Should().Be(512);
        outputImage.Height.Should().Be(512);
    }

    [Fact]
    public async Task ResizeToSquareAsync_WithPngFormat_ReturnsSuccess()
    {
        // Arrange
        var originalImageData = CreateTestImage(400, 300, new PngEncoder());
        const int targetSize = 512;
        const string resizeMode = "fit";
        const string extension = ".png";

        // Act
        var result = await _service.ResizeToSquareAsync(originalImageData, targetSize, resizeMode, extension);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ResizeToSquareAsync_InvalidImageData_ReturnsFailure()
    {
        // Arrange
        var invalidImageData = new byte[] { 1, 2, 3, 4, 5 }; // 無効な画像データ
        const int targetSize = 512;
        const string resizeMode = "fit";
        const string extension = ".jpg";

        // Act
        var result = await _service.ResizeToSquareAsync(invalidImageData, targetSize, resizeMode, extension);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("IMAGE_LOAD_ERROR");
    }

    [Fact]
    public async Task ResizeToSquareAsync_SquareImage_WithFitMode_ReturnsSuccess()
    {
        // Arrange
        var originalImageData = CreateTestImage(512, 512, new JpegEncoder());
        const int targetSize = 512;
        const string resizeMode = "fit";
        const string extension = ".jpg";

        // Act
        var result = await _service.ResizeToSquareAsync(originalImageData, targetSize, resizeMode, extension);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task ResizeToSquareAsync_VerticalImage_WithFitMode_ReturnsSuccess()
    {
        // Arrange
        var originalImageData = CreateTestImage(300, 800, new JpegEncoder());
        const int targetSize = 512;
        const string resizeMode = "fit";
        const string extension = ".jpg";

        // Act
        var result = await _service.ResizeToSquareAsync(originalImageData, targetSize, resizeMode, extension);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // 出力画像のサイズを確認
        using var outputImage = Image.Load(result.Value);
        outputImage.Width.Should().Be(512);
        outputImage.Height.Should().Be(512);
    }

    [Fact]
    public async Task ResizeToSquareAsync_HorizontalImage_WithCropMode_ReturnsSuccess()
    {
        // Arrange
        var originalImageData = CreateTestImage(1920, 1080, new JpegEncoder());
        const int targetSize = 512;
        const string resizeMode = "crop";
        const string extension = ".jpg";

        // Act
        var result = await _service.ResizeToSquareAsync(originalImageData, targetSize, resizeMode, extension);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // 出力画像のサイズを確認
        using var outputImage = Image.Load(result.Value);
        outputImage.Width.Should().Be(512);
        outputImage.Height.Should().Be(512);
    }

    [Fact]
    public void IsSupportedFormat_WithJpgExtension_ReturnsTrue()
    {
        // Arrange
        var filePath = "test.jpg";

        // Act
        var result = _service.IsSupportedFormat(filePath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSupportedFormat_WithPngExtension_ReturnsTrue()
    {
        // Arrange
        var filePath = "test.png";

        // Act
        var result = _service.IsSupportedFormat(filePath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSupportedFormat_WithBmpExtension_ReturnsTrue()
    {
        // Arrange
        var filePath = "test.bmp";

        // Act
        var result = _service.IsSupportedFormat(filePath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSupportedFormat_WithGifExtension_ReturnsFalse()
    {
        // Arrange
        var filePath = "test.gif";

        // Act
        var result = _service.IsSupportedFormat(filePath);

        // Assert
        result.Should().BeFalse();
    }
}
