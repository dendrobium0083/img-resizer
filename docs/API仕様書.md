# API仕様書

## 文書情報

| 項目 | 内容 |
|------|------|
| 文書名 | API仕様書 |
| プロジェクト名 | img-resizer |
| バージョン | 1.0.0 |
| 作成日 | 2024年 |
| 最終更新日 | 2024年 |
| 作成者 | - |
| 承認者 | - |
| 関連ドキュメント | [要件定義書](./要件定義書.md), [機能仕様書](./機能仕様書.md), [データ仕様書](./データ仕様書.md) |

---

## 1. 概要

### 1.1 APIの目的
img-resizer APIは、指定されたファイルパスの画像を512×512ピクセルの正方形に変換するRESTful WebAPIです。2種類の変換方式（全体変換方式・中央クロップ方式）をサポートします。

### 1.2 ベースURL
```
開発環境: http://localhost:5000
本番環境: （要設定）
```

### 1.3 APIバージョン
- 現在のバージョン: v1.0.0
- バージョニング方式: URLパスにバージョンを含めない（将来の拡張時に検討）

### 1.4 データ形式
- リクエスト: JSON（`Content-Type: application/json`）
- レスポンス: JSON（`Content-Type: application/json`）
- 文字エンコーディング: UTF-8

---

## 2. 認証・認可

### 2.1 認証方式
現時点では認証・認可機能は実装されていません。すべてのエンドポイントは認証なしでアクセス可能です。

### 2.2 将来の拡張
- APIキー認証
- JWT認証
- OAuth 2.0

---

## 3. エンドポイント一覧

| メソッド | エンドポイント | 説明 |
|---------|---------------|------|
| POST | `/api/image/resize` | 画像を512×512の正方形に変換 |

---

## 4. エンドポイント詳細

### 4.1 POST /api/image/resize

指定されたファイルパスの画像を512×512ピクセルの正方形に変換します。

#### 4.1.1 エンドポイント情報

- **URL**: `/api/image/resize`
- **メソッド**: `POST`
- **Content-Type**: `application/json`
- **認証**: 不要

#### 4.1.2 リクエスト

**リクエストヘッダー**:
```
Content-Type: application/json
```

**リクエストボディ（全体変換方式）**:
```json
{
  "filePath": "C:\\images\\input\\photo.jpg",
  "resizeMode": "fit"
}
```

**リクエストボディ（中央クロップ方式）**:
```json
{
  "filePath": "C:\\images\\input\\photo.jpg",
  "resizeMode": "crop"
}
```

**リクエストボディ（resizeMode省略時）**:
```json
{
  "filePath": "C:\\images\\input\\photo.jpg"
}
```
※ `resizeMode`を省略した場合、デフォルトで`fit`（全体変換方式）が適用されます

**リクエストパラメータ**:

| パラメータ名 | 型 | 必須 | デフォルト値 | 説明 | 制約 |
|------------|-----|------|------------|------|------|
| filePath | string | 必須 | - | 画像ファイルのパス | 絶対パスまたは相対パス、最大長: 260文字（Windows） |
| resizeMode | string | オプション | `fit` | 変換方式 | `fit`（全体変換）または `crop`（中央クロップ） |

**変換方式の説明**:

| 値 | 説明 |
|-----|------|
| `fit` | **全体変換方式**: アスペクト比を維持したリサイズとパディング処理により、画像全体を512×512の正方形に変換します。画像全体が含まれます。 |
| `crop` | **中央クロップ方式**: 画像の中央部分を正方形として切り出し、512×512にリサイズします。画像の一部が切り取られます。 |

#### 4.1.3 レスポンス

**正常系（200 OK - 全体変換方式）**:

**HTTPステータス**: `200 OK`

**レスポンスボディ**:
```json
{
  "success": true,
  "message": "画像を512×512に変換しました",
  "outputPath": "C:\\images\\output\\photo_512x512.jpg",
  "resizeMode": "fit"
}
```

**正常系（200 OK - 中央クロップ方式）**:

**HTTPステータス**: `200 OK`

**レスポンスボディ**:
```json
{
  "success": true,
  "message": "画像を512×512に変換しました",
  "outputPath": "C:\\images\\output\\photo_512x512_crop.jpg",
  "resizeMode": "crop"
}
```

**レスポンスフィールド**:

| フィールド名 | 型 | 説明 |
|------------|-----|------|
| success | boolean | 処理が成功したかどうか |
| message | string | 処理結果のメッセージ |
| outputPath | string | 変換後の画像ファイルのパス |
| resizeMode | string | 使用された変換方式（`fit` または `crop`） |

**異常系（400 Bad Request）**:

**HTTPステータス**: `400 Bad Request`

**レスポンスボディ**:
```json
{
  "success": false,
  "message": "リクエストが不正です",
  "errorCode": "INVALID_REQUEST"
}
```

**異常系（404 Not Found）**:

**HTTPステータス**: `404 Not Found`

**レスポンスボディ**:
```json
{
  "success": false,
  "message": "ファイルが見つかりません: C:\\images\\input\\photo.jpg",
  "errorCode": "FILE_NOT_FOUND"
}
```

**異常系（500 Internal Server Error）**:

**HTTPステータス**: `500 Internal Server Error`

**レスポンスボディ**:
```json
{
  "success": false,
  "message": "サーバーエラーが発生しました",
  "errorCode": "INTERNAL_SERVER_ERROR"
}
```

**エラーレスポンスフィールド**:

| フィールド名 | 型 | 説明 |
|------------|-----|------|
| success | boolean | 常に`false` |
| message | string | エラーメッセージ |
| errorCode | string | エラーコード（後述のエラーコード一覧を参照） |

#### 4.1.4 使用例

**cURL（全体変換方式）**:
```bash
curl -X POST "http://localhost:5000/api/image/resize" \
  -H "Content-Type: application/json" \
  -d "{\"filePath\": \"C:\\\\images\\\\input\\\\photo.jpg\", \"resizeMode\": \"fit\"}"
```

**cURL（中央クロップ方式）**:
```bash
curl -X POST "http://localhost:5000/api/image/resize" \
  -H "Content-Type: application/json" \
  -d "{\"filePath\": \"C:\\\\images\\\\input\\\\photo.jpg\", \"resizeMode\": \"crop\"}"
```

**PowerShell（全体変換方式）**:
```powershell
$body = @{
    filePath = "C:\images\input\photo.jpg"
    resizeMode = "fit"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/image/resize" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

**PowerShell（中央クロップ方式）**:
```powershell
$body = @{
    filePath = "C:\images\input\photo.jpg"
    resizeMode = "crop"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/image/resize" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

**C# (.NET)**:
```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

var client = new HttpClient();
var request = new
{
    filePath = "C:\\images\\input\\photo.jpg",
    resizeMode = "fit"
};

var json = JsonSerializer.Serialize(request);
var content = new StringContent(json, Encoding.UTF8, "application/json");
var response = await client.PostAsync("http://localhost:5000/api/image/resize", content);
var responseBody = await response.Content.ReadAsStringAsync();
```

**JavaScript (fetch)**:
```javascript
fetch('http://localhost:5000/api/image/resize', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    filePath: 'C:\\images\\input\\photo.jpg',
    resizeMode: 'fit'
  })
})
.then(response => response.json())
.then(data => console.log(data))
.catch(error => console.error('Error:', error));
```

**Python (requests)**:
```python
import requests

url = "http://localhost:5000/api/image/resize"
payload = {
    "filePath": "C:\\images\\input\\photo.jpg",
    "resizeMode": "fit"
}
headers = {
    "Content-Type": "application/json"
}

response = requests.post(url, json=payload, headers=headers)
print(response.json())
```

---

## 5. エラーコード一覧

| エラーコード | HTTPステータス | 説明 | 発生条件 |
|------------|---------------|------|----------|
| `FILE_NOT_FOUND` | 404 | ファイルが見つからない | 指定されたファイルパスにファイルが存在しない |
| `INVALID_FILE_FORMAT` | 400 | サポートされていない画像形式 | ファイルの拡張子が許可リストに含まれていない |
| `FILE_READ_ERROR` | 500 | ファイル読み込みエラー | ファイルの読み込みに失敗した（権限不足、ファイル破損等） |
| `FILE_WRITE_ERROR` | 500 | ファイル書き込みエラー | ファイルの書き込みに失敗した（権限不足、ディスク容量不足等） |
| `INVALID_CONFIGURATION` | 500 | 設定ファイルが不正 | 設定ファイルの値が無効または不足している |
| `INVALID_REQUEST` | 400 | リクエストが不正 | リクエストパラメータが不正（ファイルパスが空、resizeModeが無効等） |
| `FILE_TOO_LARGE` | 400 | ファイルサイズが大きすぎる | ファイルサイズが最大制限（50MB）を超えている |
| `INTERNAL_SERVER_ERROR` | 500 | サーバー内部エラー | 予期しないエラーが発生した |

---

## 6. HTTPステータスコード

| ステータスコード | 説明 | 使用例 |
|---------------|------|--------|
| `200 OK` | リクエストが成功し、画像変換が完了した | 正常な変換処理 |
| `400 Bad Request` | リクエストが不正 | パラメータエラー、ファイル形式エラー、ファイルサイズ超過 |
| `404 Not Found` | ファイルが見つからない | 指定されたファイルパスにファイルが存在しない |
| `500 Internal Server Error` | サーバー内部エラー | 予期しないエラー、ファイル読み書きエラー、設定エラー |

---

## 7. リクエストバリデーション

### 7.1 ファイルパスの検証

- ファイルパスは空文字列であってはならない
- ファイルパスに `..` や `~` などの危険な文字列が含まれていないか確認
- ファイルパスは正規化される（`Path.GetFullPath()`を使用）

### 7.2 拡張子の検証

- 許可された拡張子のみ処理対象とする
- 許可リスト: `.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`
- 拡張子の比較は大文字小文字を区別しない

### 7.3 resizeModeの検証

- `resizeMode`は `fit` または `crop` のみ有効
- 省略した場合、デフォルトで `fit` が適用される
- 大文字小文字を区別しない（将来的に実装）

### 7.4 ファイルサイズの検証

- 最大ファイルサイズ: 50MB（52,428,800バイト）
- 制限を超える場合は `FILE_TOO_LARGE` エラーを返す

---

## 8. 変換方式の詳細

### 8.1 全体変換方式（fit）

**説明**: アスペクト比を維持したリサイズとパディング処理により、画像全体を512×512の正方形に変換します。

**処理内容**:
1. 元画像のアスペクト比を計算
2. 幅と高さのうち大きい方に合わせて512pxにスケール
3. 512×512の正方形キャンバスを作成
4. リサイズした画像を中央に配置
5. 余白をパディング色（黒: RGB(0,0,0)）で埋める

**出力ファイル名**: `{元のファイル名}_512x512.{拡張子}`

**例**:
- 入力: `photo.jpg` (1920×1080)
- 出力: `photo_512x512.jpg` (512×512、上下にパディング)

### 8.2 中央クロップ方式（crop）

**説明**: 画像の中央部分を正方形として切り出し、512×512にリサイズします。

**処理内容**:
1. 元画像の幅と高さを取得
2. 幅と高さのうち小さい方をクロップサイズとする
3. 中央位置を計算
4. 中央部分を正方形として切り出す
5. 切り出した画像を512×512にリサイズ

**出力ファイル名**: `{元のファイル名}_512x512_crop.{拡張子}`

**例**:
- 入力: `photo.jpg` (1920×1080)
- 出力: `photo_512x512_crop.jpg` (512×512、中央1080×1080を切り出し)

**注意事項**:
- 元画像が512×512より小さい場合は、そのまま512×512に拡大する（クロップなし）

---

## 9. レート制限

現時点ではレート制限は実装されていません。

### 9.1 将来の拡張
- リクエスト数制限（例: 1分間に10リクエスト）
- IPアドレスベースの制限
- APIキーベースの制限

---

## 10. パフォーマンス

### 10.1 処理時間目標

| 画像サイズ | 目標処理時間 |
|-----------|------------|
| 1MB以下 | 1秒以内 |
| 10MB以下 | 10秒以内 |
| 50MB以下 | 30秒以内 |

### 10.2 同時リクエスト対応

- 最大同時リクエスト数: 10リクエスト
- リクエストキューイング: ASP.NET Coreのデフォルト動作に従う

---

## 11. サポートされている画像形式

| 形式 | 拡張子 | 説明 |
|------|--------|------|
| JPEG | `.jpg`, `.jpeg` | 写真に適した形式 |
| PNG | `.png` | 透明度をサポート |
| GIF | `.gif` | アニメーションGIFは最初のフレームのみ処理 |
| BMP | `.bmp` | Windows標準形式 |

---

## 12. セキュリティ考慮事項

### 12.1 パストラバーサル攻撃の防止

- ファイルパスの正規化と検証を実施
- 危険な文字列（`..`, `~`等）のチェック

### 12.2 ファイルサイズ制限

- 最大ファイルサイズ: 50MB
- 制限を超える場合はエラーを返す

### 12.3 拡張子のホワイトリスト検証

- 許可された拡張子のみ処理
- 拡張子の比較は大文字小文字を区別しない

---

## 13. Swagger/OpenAPI

開発環境では、Swagger UIが利用可能です：

- **URL**: `http://localhost:5000/swagger`（HTTP）
- **URL**: `https://localhost:5001/swagger`（HTTPS）

Swagger UIでは、APIの詳細仕様とインタラクティブなテストが可能です。

---

## 14. 変更履歴

| バージョン | 日付 | 変更内容 | 変更者 |
|-----------|------|----------|--------|
| 1.0.0 | - | 初版作成 | - |
| 1.1.0 | - | 中央クロップ方式を追加 | - |

---

## 15. 関連ドキュメント

- [要件定義書](./要件定義書.md)
- [機能仕様書](./機能仕様書.md)
- [基本設計書](./基本設計書.md)
- [詳細設計書](./詳細設計書.md)
- [データ仕様書](./データ仕様書.md)
- [README.md](../README.md)

---

## 16. サポート・問い合わせ

APIに関する問い合わせや問題報告は、プロジェクトのIssueトラッカーで受け付けます。

---

## 付録A: リクエスト/レスポンススキーマ

### A.1 リクエストスキーマ

```json
{
  "type": "object",
  "required": ["filePath"],
  "properties": {
    "filePath": {
      "type": "string",
      "description": "画像ファイルのパス",
      "maxLength": 260
    },
    "resizeMode": {
      "type": "string",
      "enum": ["fit", "crop"],
      "default": "fit",
      "description": "変換方式"
    }
  }
}
```

### A.2 レスポンススキーマ（成功）

```json
{
  "type": "object",
  "required": ["success", "message", "outputPath", "resizeMode"],
  "properties": {
    "success": {
      "type": "boolean",
      "description": "処理が成功したかどうか",
      "example": true
    },
    "message": {
      "type": "string",
      "description": "処理結果のメッセージ",
      "example": "画像を512×512に変換しました"
    },
    "outputPath": {
      "type": "string",
      "description": "変換後の画像ファイルのパス",
      "example": "C:\\images\\output\\photo_512x512.jpg"
    },
    "resizeMode": {
      "type": "string",
      "enum": ["fit", "crop"],
      "description": "使用された変換方式",
      "example": "fit"
    }
  }
}
```

### A.3 レスポンススキーマ（エラー）

```json
{
  "type": "object",
  "required": ["success", "message", "errorCode"],
  "properties": {
    "success": {
      "type": "boolean",
      "description": "常にfalse",
      "example": false
    },
    "message": {
      "type": "string",
      "description": "エラーメッセージ",
      "example": "ファイルが見つかりません: C:\\images\\input\\photo.jpg"
    },
    "errorCode": {
      "type": "string",
      "description": "エラーコード",
      "example": "FILE_NOT_FOUND"
    }
  }
}
```

---

## 付録B: エラーレスポンス例

### B.1 ファイルが見つからない場合

**リクエスト**:
```json
{
  "filePath": "C:\\images\\input\\nonexistent.jpg"
}
```

**レスポンス**:
```json
{
  "success": false,
  "message": "ファイルが見つかりません: C:\\images\\input\\nonexistent.jpg",
  "errorCode": "FILE_NOT_FOUND"
}
```

### B.2 無効な拡張子の場合

**リクエスト**:
```json
{
  "filePath": "C:\\images\\input\\document.pdf",
  "resizeMode": "fit"
}
```

**レスポンス**:
```json
{
  "success": false,
  "message": "サポートされていない画像形式です: .pdf",
  "errorCode": "INVALID_FILE_FORMAT"
}
```

### B.3 無効なresizeModeの場合

**リクエスト**:
```json
{
  "filePath": "C:\\images\\input\\photo.jpg",
  "resizeMode": "invalid"
}
```

**レスポンス**:
```json
{
  "success": false,
  "message": "無効な変換方式です: invalid。有効な値は 'fit' または 'crop' です。",
  "errorCode": "INVALID_REQUEST"
}
```

### B.4 ファイルサイズ超過の場合

**リクエスト**:
```json
{
  "filePath": "C:\\images\\input\\large.jpg",
  "resizeMode": "fit"
}
```

**レスポンス**:
```json
{
  "success": false,
  "message": "ファイルサイズが大きすぎます。最大サイズは50MBです。",
  "errorCode": "FILE_TOO_LARGE"
}
```

