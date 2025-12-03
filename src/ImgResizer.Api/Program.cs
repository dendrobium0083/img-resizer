using FluentValidation;
using ImgResizer.Api.Middleware;
using ImgResizer.Application.Behaviors;
using ImgResizer.Application.Commands;
using ImgResizer.Application.Validators;
using ImgResizer.Domain.Interfaces;
using ImgResizer.Infrastructure.Configuration;
using ImgResizer.Infrastructure.Repositories;
using ImgResizer.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

// Serilogの初期設定（起動前）
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProcessId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/img-resizer-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 100_000_000, // 100MB
        rollOnFileSizeLimit: true)
    .CreateBootstrapLogger(); // 起動時のログ用

try
{
    Log.Information("アプリケーション起動中...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilogをロガーとして使用
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithProcessId());

    // 設定の読み込みと検証
    builder.Services.AddOptions<ImageResizeSettings>()
        .Bind(builder.Configuration.GetSection("ImageResize"))
        .ValidateOnStart();

    // 設定検証クラスの登録
    builder.Services.AddSingleton<IValidateOptions<ImageResizeSettings>, ImageResizeSettingsValidator>();

    // サービスの登録
    builder.Services.AddScoped<IImageRepository, FileSystemImageRepository>();
    builder.Services.AddScoped<IImageResizeService, ImageResizeService>();

    // MediatRの登録
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(ResizeImageCommand).Assembly);
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    });

    // FluentValidationの登録
    builder.Services.AddValidatorsFromAssemblyContaining<ResizeImageCommandValidator>();

    // コントローラー
    builder.Services.AddControllers();

    // Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // グローバル例外ハンドラー（最初に登録）
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    // ミドルウェアの設定
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // リクエストログの追加
    app.UseSerilogRequestLogging(options =>
    {
        // ログレベルのカスタマイズ
        options.GetLevel = (httpContext, elapsed, ex) => ex != null
            ? LogEventLevel.Error
            : elapsed > 5000
                ? LogEventLevel.Warning
                : LogEventLevel.Information;

        // ログメッセージのカスタマイズ
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("アプリケーション正常起動完了");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "アプリケーション起動時に致命的なエラーが発生しました");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
