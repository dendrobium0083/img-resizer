using FluentValidation;
using ImgResizer.Api.Middleware;
using ImgResizer.Application.UseCases;
using ImgResizer.Application.Validators;
using ImgResizer.Domain.Interfaces;
using ImgResizer.Infrastructure.Configuration;
using ImgResizer.Infrastructure.Repositories;
using ImgResizer.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 設定の読み込み
builder.Services.Configure<ImageResizeSettings>(
    builder.Configuration.GetSection("ImageResize"));

// サービスの登録
builder.Services.AddScoped<IImageRepository, FileSystemImageRepository>();
builder.Services.AddScoped<IImageResizeService, ImageResizeService>();
builder.Services.AddScoped<ResizeImageUseCase>();

// FluentValidationの登録
builder.Services.AddValidatorsFromAssemblyContaining<ResizeImageRequestValidator>();

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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
