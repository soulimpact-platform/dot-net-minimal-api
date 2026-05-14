# ログイン認証サンプルアプリ

## 概要

HTML / JavaScript、ASP.NET Core Minimal API、SQLite を使用したログイン認証サンプルです。

ログイン画面で入力されたユーザー名・パスワードを、SQLite の `users` テーブルと照合します。  
認証に成功すると、アカウント表示画面でログインユーザー名を表示します。

## 使用技術

- HTML / JavaScript
- C# / ASP.NET Core Minimal API
- SQLite
- Docker

## 初期登録ユーザー

| ユーザー名 | パスワード |
|---|---|
| user01 | password01 |
| user02 | password02 |
| user03 | password03 |
| user04 | password04 |

## 起動手順

Dockerイメージを作成します。

```powershell
docker build -t login-sample-api .
```

Dockerコンテナを起動します。

```powershell
docker run --rm -p 8080:8080 login-sample-api
```

ブラウザで以下にアクセスします。

```text
http://localhost:8080/login.html
```

## 画面

- `login.html`：ログイン画面
- `account.html`：ログイン後のアカウント表示画面

## API

### ログインAPI

```text
POST /api/login
```

入力されたユーザー名・パスワードを `users` テーブルと照合します。