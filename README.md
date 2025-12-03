# img-resizer

指定されたファイルパスの画像を512×512の正方形に変換するWebAPIアプリケーションです。

## 概要

このプロジェクトは、クリーンアーキテクチャに基づいて設計されたASP.NET Core WebAPIアプリケーションです。設定ファイルで指定されたファイルパスの画像を512×512の正方形に変換する機能を提供します。

## 技術スタック

- **フレームワーク**: .NET 8
- **言語**: C#
- **アーキテクチャ**: クリーンアーキテクチャ + CQRS
- **API**: ASP.NET Core WebAPI
- **画像処理**: SixLabors.ImageSharp ✅（実装完了: 2025-12-02）
- **エラーハンドリング**: Resultパターン + グローバル例外ハンドラー ✅（実装完了: 2025-12-01）
- **ロギング**: ✅ Serilog（実装完了: 2025-12-02）
- **バリデーション**: FluentValidation ✅（実装完了: 2025-12-01）
- **メディエーター**: MediatR ✅（実装完了: 2025-12-02）
- **静的解析**: ✅ Roslyn Analyzers、StyleCop、SonarAnalyzer、Roslynator（実装完了: 2025-12-01、警告0個）
- **ドキュメント**: ✅ XMLドキュメントコメント（実装完了: 2025-12-01）

## 機能

- 指定されたファイルパスの画像を512×512の正方形に変換
- **2種類の変換方式をサポート**:
  - **全体変換方式（fit）**: アスペクト比を維持したリサイズ + パディング
  - **中央クロップ方式（crop）**: 画像の中央部分を切り出してリサイズ
- 複数の画像形式に対応（JPEG、PNG、GIF、BMP）
- 設定ファイル（`appsettings.json`）からファイルパスを取得
- RESTful APIエンドポイント
- Swagger/OpenAPIドキュメント
- ✅ 構造化ログ（Serilog）によるトラブルシューティング（実装完了: 2025-12-02）
- FluentValidationによる堅牢なバリデーション
- Resultパターンによる明示的なエラーハンドリング
- ✅ 設定値の起動時検証（実装完了: 2025-12-02）

## プロジェクト構造

```
img-resizer/
├── src/
│   ├── ImgResizer.Api/              # Presentation層（WebAPI）
│   ├── ImgResizer.Application/       # Application層（ユースケース）
│   ├── ImgResizer.Domain/            # Domain層（エンティティ、インターフェース）
│   └── ImgResizer.Infrastructure/    # Infrastructure層（実装）
└── docs/
    ├── 要件定義書.md                  # スペック開発：要件定義
    ├── 機能仕様書.md                  # スペック開発：機能仕様
    ├── API仕様書.md                   # スペック開発：API仕様
    ├── データ仕様書.md                # スペック開発：データ仕様
    ├── 基本設計書.md                  # スペック開発：基本設計
    ├── 詳細設計書.md                  # スペック開発：詳細設計
    ├── エラーハンドリング仕様書.md     # スペック開発：エラーハンドリング
    ├── リファクタリング改善案.md       # モダンな設計への移行計画
    ├── ドキュメント一覧.md             # ドキュメント管理
    └── （その他の開発ガイド）
```

## API仕様

### エンドポイント

#### POST /api/image/resize

指定されたファイルパスの画像を512×512の正方形に変換します。

**リクエスト（全体変換方式）**:
```json
{
  "filePath": "C:\\images\\input.jpg",
  "resizeMode": "fit"
}
```

**リクエスト（中央クロップ方式）**:
```json
{
  "filePath": "C:\\images\\input.jpg",
  "resizeMode": "crop"
}
```

**リクエストパラメータ**:
| 項目 | 型 | 必須 | 説明 | 値 |
|------|-----|------|------|-----|
| filePath | string | 必須 | 画像ファイルのパス | - |
| resizeMode | string | オプション | 変換方式 | `fit`（全体変換、デフォルト）または `crop`（中央クロップ） |

**レスポンス（成功 - 全体変換方式）**:
```json
{
  "success": true,
  "message": "画像を512×512に変換しました",
  "outputPath": "C:\\images\\output_512x512.jpg",
  "resizeMode": "fit"
}
```

**レスポンス（成功 - 中央クロップ方式）**:
```json
{
  "success": true,
  "message": "画像を512×512に変換しました",
  "outputPath": "C:\\images\\output_512x512_crop.jpg",
  "resizeMode": "crop"
}
```

**レスポンス（エラー）**:
```json
{
  "success": false,
  "message": "ファイルが見つかりません: C:\\images\\input.jpg",
  "errorCode": "FILE_NOT_FOUND"
}
```

**HTTPステータスコード**:
- `200 OK`: 変換成功
- `400 Bad Request`: リクエストが不正
- `404 Not Found`: ファイルが見つからない
- `500 Internal Server Error`: サーバーエラー

## 設定ファイル

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "ImgResizer": "Debug"
    }
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "ImgResizer": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/img-resizer-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "fileSizeLimitBytes": 104857600,
          "rollOnFileSizeLimit": true
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/img-resizer-errors-.log",
          "rollingInterval": "Day",
          "restrictedToMinimumLevel": "Error",
          "retainedFileCountLimit": 90
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId", "WithProcessId" ]
  },
  "ImageResize": {
    "InputDirectory": "C:\\images\\input",
    "OutputDirectory": "C:\\images\\output",
    "TargetSize": {
      "Width": 512,
      "Height": 512
    },
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".bmp"],
    "MaxFileSize": 52428800,
    "PaddingColor": {
      "R": 0,
      "G": 0,
      "B": 0,
      "A": 255
    },
    "ImageQuality": {
      "JpegQuality": 90,
      "PngCompressionLevel": 6
    }
  }
}
```

**注意**: 設定値は起動時に検証されます。無効な設定がある場合、アプリケーションは起動を停止します。

## 必要な環境

- .NET 8.0 SDK 以上

## ビルド方法

```bash
dotnet build
```

## 実行方法

```bash
dotnet run --project src/ImgResizer.Api/ImgResizer.Api.csproj
```

開発環境では、Swagger UIが利用可能です：
- URL: `https://localhost:5001/swagger`（HTTPS）
- URL: `http://localhost:5000/swagger`（HTTP）

## 使用方法

### cURL の例

```bash
curl -X POST "https://localhost:5001/api/image/resize" \
  -H "Content-Type: application/json" \
  -d "{\"filePath\": \"C:\\\\images\\\\input.jpg\"}"
```

### PowerShell の例

```powershell
$body = @{
    filePath = "C:\images\input.jpg"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/image/resize" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

## アーキテクチャ

このプロジェクトはクリーンアーキテクチャの原則に基づいて設計されています：

- **Domain層**: ビジネスエンティティとインターフェース（他のレイヤーに依存しない）
- **Application層**: ユースケースの実装（Domain層のみに依存）
  - CQRSパターン（Command/Query分離）
  - MediatRによるメディエーターパターン
  - FluentValidationによるバリデーション
  - Resultパターンによるエラーハンドリング
- **Infrastructure層**: 外部ライブラリの使用と実装（Domain層に依存）
  - ImageSharpによる画像処理
  - ファイルシステムアクセス
  - ✅ Serilogによる構造化ログ（実装完了: 2025-12-02）
  - ✅ 設定値の検証（ImageResizeSettingsValidator、実装完了: 2025-12-02）
- **Presentation層**: WebAPIコントローラー（Application層とInfrastructure層に依存）
  - RESTful API
  - グローバル例外ハンドラー（実装済み）
  - Swagger/OpenAPI

### モダンな設計パターン

- **Resultパターン**: 例外駆動からResult駆動へ移行
- **CQRSパターン**: Command/Queryの明確な分離
- **MediatRパイプライン**: バリデーション、ログ、トランザクション管理
- **ドメインイベント**: ビジネスイベントの明示的な表現
- **構造化ログ**: Serilogによる高度なログ分析

詳細は `docs/基本設計書.md`、`docs/詳細設計書.md`、および `docs/リファクタリング改善案.md` を参照してください。

## ドキュメント

このプロジェクトはスペック開発（Specification-driven Development）に基づいて開発されています。以下のドキュメントが利用可能です：

### スペック開発ドキュメント

- [要件定義書](docs/要件定義書.md) - プロジェクトの目的、機能要件、非機能要件
- [機能仕様書](docs/機能仕様書.md) - 各機能の詳細な動作仕様
- [API仕様書](docs/API仕様書.md) - RESTful APIの詳細仕様
- [データ仕様書](docs/データ仕様書.md) - データモデルとデータ構造
- [基本設計書](docs/基本設計書.md) - アーキテクチャと設計方針（概要レベル）
- [詳細設計書](docs/詳細設計書.md) - 詳細な実装設計
- [エラーハンドリング仕様書](docs/エラーハンドリング仕様書.md) - エラー処理の方針と実装仕様
- [リファクタリング改善案](docs/リファクタリング改善案.md) - モダンな設計への移行計画

### 開発ガイド

- [ドキュメント一覧](docs/ドキュメント一覧.md) - ドキュメントの全体像と作成状況
- [クイックスタートガイド](docs/クイックスタートガイド.md) - プロジェクトのセットアップ手順
- [Gitセットアップガイド](docs/Gitセットアップガイド.md) - Gitリポジトリのセットアップ
- [GitHub認証トラブルシューティング](docs/GitHub認証トラブルシューティング.md) - GitHub認証の問題解決

詳細なAPI仕様については、[API仕様書](docs/API仕様書.md)を参照してください。開発環境では、Swagger UI（`/swagger`）でもAPI仕様を確認できます。

## コード品質管理

このプロジェクトでは、以下のツールを使用してコード品質を維持しています：

### 静的解析ツール

- **.editorconfig**: コーディングスタイルの統一 ✅
- **Roslyn Analyzers**: .NET標準の静的解析（AnalysisMode: All）✅
- **StyleCop Analyzers**: コーディング規約の強制 ✅
- **SonarAnalyzer.CSharp**: バグ・脆弱性検出 ✅
- **Roslynator**: コード改善提案 ✅

**現在の警告数: 0個**（2025-12-01達成）

### ドキュメント

- **XMLドキュメントコメント**: すべてのpublicメンバーに完備 ✅
  - IntelliSenseでコメント表示に対応
  - クラス、メソッド、プロパティの説明を含む

### ロギング

- ✅ **Serilog**: 構造化ログによる高度なログ分析（実装完了: 2025-12-02）
  - コンソール出力（開発環境）
  - ファイル出力（本番環境、ローテーション付き、30日保持）
  - エラーログ専用ファイル（90日保持）
  - Seq統合（開発環境でのログ可視化）
  - リクエストログの自動記録
  - ログエンリッチャー（MachineName、ThreadId、ProcessId）

### バリデーション

- **FluentValidation**: 宣言的で読みやすいバリデーションルール
- **MediatRパイプライン**: 自動バリデーション実行

### 設定管理

- ✅ **設定値の起動時検証**（実装完了: 2025-12-02）
  - `ImageResizeSettingsValidator`による設定値検証
  - 無効な設定がある場合、アプリケーション起動を停止
  - 設定検証の単体テストを実装

## 今後の改善計画

モダンな設計パターンへの段階的な移行を計画しています。詳細は [リファクタリング改善案](docs/リファクタリング改善案.md) を参照してください：

### フェーズ1: 基盤改善（✅ 完全完了: 2025-12-01）
- ✅ **Resultパターンの導入**（**完了: 2025-12-01**）
- ✅ **グローバル例外ハンドラーの実装**（**完了: 2025-12-01**）
- ✅ **FluentValidationの導入**（**完了: 2025-12-01**）
- ✅ **静的解析ツールの導入**（**完了: 2025-12-01**）
- ✅ **静的解析の全警告修正**（**完了: 2025-12-01**）- 警告数: 237個 → 0個
- ✅ **XMLドキュメントコメント追加**（**完了: 2025-12-01**）

### フェーズ2: アーキテクチャ改善（✅ 完全完了: 2025-12-02）
- ✅ **MediatR + CQRS実装**（**完了: 2025-12-02**）

### フェーズ3: ライブラリ移行（✅ 完全完了: 2025-12-02）
- ✅ **ImageSharp導入**（**完了: 2025-12-02**）- System.Drawing.Commonから移行完了

### フェーズ5: 設定・ロギング強化（✅ 完全完了: 2025-12-02）
- ✅ **設定クラスの強化**（**完了: 2025-12-02**）- IValidateOptionsによる起動時検証
- ✅ **Serilog導入**（**完了: 2025-12-02**）- 構造化ログ、リクエストログ、複数出力先対応

### 優先度: 低
- 📋 ドメインイベントの導入
- 📋 Minimal API検討

## ライセンス

このプロジェクトはサンプルコードです。

