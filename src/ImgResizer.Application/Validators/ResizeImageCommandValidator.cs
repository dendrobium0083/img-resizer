using System.Globalization;
using FluentValidation;
using ImgResizer.Application.Commands;
using ImgResizer.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ImgResizer.Application.Validators;

/// <summary>
/// ResizeImageCommandのバリデーター。
/// MediatRパイプラインでバリデーションを実行するために使用されます。
/// </summary>
public class ResizeImageCommandValidator : AbstractValidator<ResizeImageCommand>
{
    private static readonly string[] ValidResizeModes = { "fit", "crop" };

    private readonly IOptions<ImageResizeSettings> _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResizeImageCommandValidator"/> class.
    /// </summary>
    /// <param name="settings">画像リサイズ設定。</param>
    public ResizeImageCommandValidator(IOptions<ImageResizeSettings> settings)
    {
        _settings = settings;

        // FilePath必須チェック
        RuleFor(x => x.FilePath)
            .NotEmpty()
            .WithMessage("ファイルパスが指定されていません")
            .WithErrorCode("VALIDATION_ERROR");

        // パストラバーサル攻撃防止
        RuleFor(x => x.FilePath)
            .Must(path => !path.Contains("..", StringComparison.Ordinal) && !path.Contains('~'))
            .WithMessage("無効なファイルパスです")
            .WithErrorCode("VALIDATION_ERROR")
            .When(x => !string.IsNullOrEmpty(x.FilePath));

        // ResizeModeの検証
        RuleFor(x => x.ResizeMode)
            .Must(mode => string.IsNullOrEmpty(mode) ||
                         ValidResizeModes.Contains(mode.ToLower(CultureInfo.InvariantCulture)))
            .WithMessage(x => $"無効な変換方式です: {x.ResizeMode}。有効な値は 'fit' または 'crop' です")
            .WithErrorCode("VALIDATION_ERROR");

        // ファイルの存在確認
        RuleFor(x => x.FilePath)
            .Must(File.Exists)
            .WithMessage(x => $"ファイルが見つかりません: {x.FilePath}")
            .WithErrorCode("FILE_NOT_FOUND")
            .When(x => !string.IsNullOrEmpty(x.FilePath) &&
                      !x.FilePath.Contains("..", StringComparison.Ordinal) &&
                      !x.FilePath.Contains('~'));

        // 拡張子の検証
        RuleFor(x => x.FilePath)
            .Must(HasValidExtension)
            .WithMessage(x => $"サポートされていない画像形式です: {Path.GetExtension(x.FilePath)}")
            .WithErrorCode("UNSUPPORTED_FORMAT")
            .When(x => !string.IsNullOrEmpty(x.FilePath) && File.Exists(x.FilePath));

        // ファイルサイズの検証
        RuleFor(x => x.FilePath)
            .Must(IsFileSizeValid)
            .WithMessage(x => $"ファイルサイズが大きすぎます。上限: {_settings.Value.MaxFileSize / (1024 * 1024)}MB")
            .WithErrorCode("FILE_TOO_LARGE")
            .When(x => !string.IsNullOrEmpty(x.FilePath) && File.Exists(x.FilePath));
    }

    /// <summary>
    /// ファイルの拡張子が許可されているかを検証する。
    /// </summary>
    /// <param name="filePath">ファイルパス。</param>
    /// <returns>許可されている拡張子の場合true。</returns>
    private bool HasValidExtension(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower(CultureInfo.InvariantCulture);
        return _settings.Value.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// ファイルサイズが許可範囲内かを検証する。
    /// </summary>
    /// <param name="filePath">ファイルパス。</param>
    /// <returns>許可範囲内の場合true。</returns>
    private bool IsFileSizeValid(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return true;
        }

        var fileInfo = new FileInfo(filePath);
        return fileInfo.Length <= _settings.Value.MaxFileSize;
    }
}
