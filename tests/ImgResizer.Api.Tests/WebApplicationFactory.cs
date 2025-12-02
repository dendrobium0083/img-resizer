using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ImgResizer.Api.Tests;

/// <summary>
/// 統合テスト用のWebApplicationFactory。
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// テスト用の設定を構成します。
    /// </summary>
    /// <param name="builder">Webホストビルダー。</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // テスト用の設定を追加
            var testConfig = new Dictionary<string, string?>
            {
                { "ImageResize:InputDirectory", Path.Combine(Path.GetTempPath(), "img-resizer-test", "input") },
                { "ImageResize:OutputDirectory", Path.Combine(Path.GetTempPath(), "img-resizer-test", "output") },
                { "ImageResize:TargetSize:Width", "512" },
                { "ImageResize:TargetSize:Height", "512" },
                { "ImageResize:AllowedExtensions:0", ".jpg" },
                { "ImageResize:AllowedExtensions:1", ".jpeg" },
                { "ImageResize:AllowedExtensions:2", ".png" },
                { "ImageResize:AllowedExtensions:3", ".bmp" },
                { "ImageResize:MaxFileSize", "10485760" } // 10MB
            };

            config.AddInMemoryCollection(testConfig);
        });

        builder.UseEnvironment("Testing");
    }
}

