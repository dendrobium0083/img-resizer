# img-resizer

画像のリサイズ（サイズ変更）を行うC#コンソールアプリケーションです。

## 機能

- 画像の幅・高さを指定してリサイズ
- アスペクト比を維持したリサイズ
- 複数の画像形式に対応（JPEG、PNG、GIF、BMP）

## 使用方法

```bash
dotnet run -- <入力ファイル> <出力ファイル> [幅] [高さ]
```

### 例

```bash
# 幅800px、高さ600pxにリサイズ
dotnet run -- input.jpg output.jpg 800 600

# 幅のみ指定（アスペクト比維持）
dotnet run -- input.jpg output.jpg 800

# 高さのみ指定（アスペクト比維持）
dotnet run -- input.jpg output.jpg 0 600
```

## 必要な環境

- .NET 8.0 SDK 以上

## ビルド方法

```bash
dotnet build
```

## 実行方法

```bash
dotnet run
```

## Git管理

このプロジェクトはGitで管理されています。詳細は `GIT_SETUP_GUIDE.md` を参照してください。

