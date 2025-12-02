using ImgResizer.Application.DTOs;
using ImgResizer.Domain.Common;
using MediatR;

namespace ImgResizer.Application.Commands;

/// <summary>
/// 画像リサイズコマンド。
/// MediatRを使用してCQRSパターンでリクエストを処理します。
/// </summary>
/// <param name="FilePath">リサイズ対象の画像ファイルパス。</param>
/// <param name="ResizeMode">リサイズモード（"fit": アスペクト比を維持してパディング、"crop": 中央部分を切り出し）。</param>
public record ResizeImageCommand(string FilePath, string? ResizeMode) : IRequest<Result<ResizeImageResponse>>;
