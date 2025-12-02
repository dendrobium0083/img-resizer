using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ImgResizer.Api.Tests;
using ImgResizer.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace ImgResizer.Api.Tests.Controllers;

/// <summary>
/// ImageControllerの統合テスト。
/// </summary>
public class ImageControllerTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly string _testDirectory;
    private readonly string _testInputDirectory;
    private readonly string _testOutputDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageControllerTests"/> class.
    /// </summary>
    /// <param name="factory">Webアプリケーションファクトリー。</param>
    public ImageControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // テスト用のディレクトリを作成
        _testDirectory = Path.Combine(Path.GetTempPath(), "img-resizer-test");
        _testInputDirectory = Path.Combine(_testDirectory, "input");
        _testOutputDirectory = Path.Combine(_testDirectory, "output");

        Directory.CreateDirectory(_testInputDirectory);
        Directory.CreateDirectory(_testOutputDirectory);
    }

    /// <summary>
    /// テスト用リソースをクリーンアップします。
    /// </summary>
    public void Dispose()
    {
        _client.Dispose();
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    /// <summary>
    /// テスト用の画像ファイルを作成します。
    /// </summary>
    /// <param name="fileName">ファイル名。</param>
    /// <param name="width">幅。</param>
    /// <param name="height">高さ。</param>
    /// <param name="encoder">画像エンコーダー。</param>
    /// <returns>作成されたファイルのパス。</returns>
    private string CreateTestImageFile(string fileName, int width, int height, IImageEncoder encoder)
    {
        var filePath = Path.Combine(_testInputDirectory, fileName);
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

        image.Save(filePath, encoder);
        return filePath;
    }

    [Fact]
    public async Task ResizeImage_ValidRequest_Returns200Ok()
    {
        // Arrange
        var testImagePath = CreateTestImageFile("test.jpg", 800, 600, new JpegEncoder());
        var request = new ResizeImageRequest
        {
            FilePath = testImagePath,
            ResizeMode = "fit"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/image/resize", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResizeImageResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.OutputPath.Should().NotBeNullOrEmpty();
        result.ResizeMode.Should().Be("fit");
    }

    [Fact]
    public async Task ResizeImage_FileNotFound_Returns404NotFound()
    {
        // Arrange
        var request = new ResizeImageRequest
        {
            FilePath = Path.Combine(_testInputDirectory, "nonexistent.jpg"),
            ResizeMode = "fit"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/image/resize", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var result = await response.Content.ReadFromJsonAsync<ResizeImageResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("FILE_NOT_FOUND");
    }

    [Fact]
    public async Task ResizeImage_ValidationError_Returns400BadRequest()
    {
        // Arrange
        var request = new ResizeImageRequest
        {
            FilePath = string.Empty, // 空のファイルパス
            ResizeMode = "fit"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/image/resize", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<ResizeImageResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task ResizeImage_UnsupportedFormat_Returns400BadRequest()
    {
        // Arrange
        // GIFファイルを作成（サポートされていない形式）
        var testImagePath = Path.Combine(_testInputDirectory, "test.gif");
        File.WriteAllBytes(testImagePath, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 });

        var request = new ResizeImageRequest
        {
            FilePath = testImagePath,
            ResizeMode = "fit"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/image/resize", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<ResizeImageResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("UNSUPPORTED_FORMAT");
    }

    [Fact]
    public async Task ResizeImage_FileTooLarge_Returns400BadRequest()
    {
        // Arrange
        // 11MBのファイルを作成（上限10MBを超える）
        var testImagePath = Path.Combine(_testInputDirectory, "large.jpg");
        var largeData = new byte[11 * 1024 * 1024];
        File.WriteAllBytes(testImagePath, largeData);

        var request = new ResizeImageRequest
        {
            FilePath = testImagePath,
            ResizeMode = "fit"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/image/resize", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<ResizeImageResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("FILE_TOO_LARGE");
    }

    [Fact]
    public async Task ResizeImage_WithCropMode_Returns200Ok()
    {
        // Arrange
        var testImagePath = CreateTestImageFile("test-crop.jpg", 1920, 1080, new JpegEncoder());
        var request = new ResizeImageRequest
        {
            FilePath = testImagePath,
            ResizeMode = "crop"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/image/resize", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResizeImageResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.ResizeMode.Should().Be("crop");
    }

    [Fact]
    public async Task ResizeImage_WithoutResizeMode_UsesDefaultFit()
    {
        // Arrange
        var testImagePath = CreateTestImageFile("test-default.jpg", 800, 600, new JpegEncoder());
        var request = new ResizeImageRequest
        {
            FilePath = testImagePath,
            ResizeMode = null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/image/resize", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResizeImageResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.ResizeMode.Should().Be("fit");
    }

    [Fact]
    public async Task ResizeImage_WithPngFormat_Returns200Ok()
    {
        // Arrange
        var testImagePath = CreateTestImageFile("test.png", 400, 300, new PngEncoder());
        var request = new ResizeImageRequest
        {
            FilePath = testImagePath,
            ResizeMode = "fit"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/image/resize", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ResizeImageResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ResizeImage_InvalidResizeMode_Returns400BadRequest()
    {
        // Arrange
        var testImagePath = CreateTestImageFile("test.jpg", 800, 600, new JpegEncoder());
        var request = new ResizeImageRequest
        {
            FilePath = testImagePath,
            ResizeMode = "invalid"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/image/resize", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<ResizeImageResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }
}
