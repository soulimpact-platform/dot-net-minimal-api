# ログイン認証サンプルアプリ

## 概要

ログイン画面で入力されたユーザー名・パスワードを、SQLite の `users` テーブルと照合します。  
ログイン後の画面で入力された書籍名・カテゴリを、SQLite の `products` テーブルと照合します。  
検索結果の書籍名をクリックすると、書籍詳細画面へ遷移します。

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

## 初期登録書籍

| ID | 書籍名 | カテゴリ | 価格 |
|---|---|---|---:|
| 1 | 独習C# | 技術書 | 3600 |
| 2 | なるほどなっとくC#入門 | 技術書 | 3000 |
| 3 | 独習JavaScript 新版 | 技術書 | 3200 |
| 4 | 吾輩は猫である | 小説 | 800 |
| 5 | 坊っちゃん | 小説 | 700 |
| 6 | 銀河鉄道の夜 | 小説 | 750 |
| 7 | 日本の歴史 | 歴史 | 1200 |
| 8 | 世界の歴史 | 歴史 | 1300 |
| 9 | 英単語ターゲット1900 | 語学 | 1100 |
| 10 | 速読英単語 必修編 | 語学 | 1200 |

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
- `account.html`：アカウント表示・書籍検索画面
- `detail.html`：書籍詳細画面

## 検索機能

ログイン後のアカウント表示画面で、書籍名とカテゴリを入力して検索します。
入力された書籍名・カテゴリを、SQLite の `products` テーブルと照合します。  
検索条件は以下の2項目で、AND条件として検索します。

- 書籍名
- カテゴリ

```sql
WHERE name LIKE $name
  AND category LIKE $category
```

## API

### ログインAPI

```text
POST /api/login
```

入力されたユーザー名・パスワードを `users` テーブルと照合します。

### 書籍検索API

```text
GET /api/products/search?name={書籍名}&category={カテゴリ}
```

入力された書籍名・カテゴリを `products` テーブルと照合します。

### 書籍詳細API

```text
GET /api/products/{id}
```

指定されたIDの書籍情報を `products` テーブルから取得します。