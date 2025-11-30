# GitHub認証エラーの解決方法

## エラーメッセージ

```
remote: Invalid username or token. Password authentication is not supported for Git operations.
fatal: Authentication failed for 'https://github.com/...'
```

## 原因

GitHubは2021年8月からパスワード認証を廃止しました。代わりに**Personal Access Token (PAT)** または**SSHキー**を使用する必要があります。

## 解決方法：Personal Access Token (PAT) を使用

### ステップ1: Personal Access Tokenを作成

1. [GitHub](https://github.com)にログイン
2. 右上のプロフィール画像をクリック → **Settings**
3. 左メニューの一番下 → **Developer settings**
4. **Personal access tokens** → **Tokens (classic)**
5. **Generate new token** → **Generate new token (classic)**
6. 以下の情報を入力：
   - **Note**: 用途を記入（例：`img-resizer project`）
   - **Expiration**: 有効期限を選択
     - 90 days（推奨：セキュリティのため）
     - No expiration（便利だがセキュリティリスクあり）
   - **Select scopes**: `repo` にチェック
     - これにより、リポジトリへの読み書きが可能になります
7. 一番下の **Generate token** をクリック
8. **重要**: 表示されたトークンをコピーしてください
   - この画面を閉じると、二度と表示されません
   - 例：`ghp_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx`

### ステップ2: トークンを使用してプッシュ

```bash
git push -u origin main
```

認証を求められたら：
- **Username**: GitHubのユーザー名（例：`dendrobium0083`）
- **Password**: 先ほどコピーした**Personal Access Token**を貼り付け

通常のパスワードではなく、トークンを入力することが重要です。

### ステップ3: 認証情報を保存（推奨）

毎回トークンを入力するのが面倒な場合は、Git Credential Managerを使用できます：

```bash
git config --global credential.helper manager-core
```

これにより、次回から自動的に認証情報が使用されます。

## 代替方法：SSHキーを使用

SSHキーを使用する方法もあります。よりセキュアですが、設定が少し複雑です。

### SSHキーの設定手順

1. SSHキーを生成（まだ持っていない場合）：
   ```bash
   ssh-keygen -t ed25519 -C "your_email@example.com"
   ```

2. SSHキーをGitHubに登録：
   - 生成された公開鍵（`~/.ssh/id_ed25519.pub`）の内容をコピー
   - GitHub → Settings → SSH and GPG keys → New SSH key

3. リモートURLをSSHに変更：
   ```bash
   git remote set-url origin git@github.com:dendrobium0083/img-resizer.git
   ```

4. プッシュ：
   ```bash
   git push -u origin main
   ```

## よくある質問

### Q: トークンはどこに保存されますか？

A: Windowsの場合、Git Credential Managerを使用している場合、Windows Credential Managerに保存されます。コントロールパネル → 資格情報マネージャー で確認できます。

### Q: トークンが漏洩したらどうすればいいですか？

A: GitHub → Settings → Developer settings → Personal access tokens で、該当するトークンを削除してください。

### Q: トークンの有効期限が切れたら？

A: 新しいトークンを作成して、再度認証してください。

## 参考リンク

- [GitHub公式ドキュメント：Personal Access Token](https://docs.github.com/ja/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token)
- [Git Credential Manager](https://github.com/GitCredentialManager/git-credential-manager)

