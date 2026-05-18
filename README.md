# ログイン認証・書籍検索サンプルアプリ

## 概要


ログイン画面で入力されたユーザー名・パスワードを、PostgreSQL の `users` テーブルと照合します。  
認証に成功するとJWTを発行し、PostgreSQL の `login_tokens` テーブルに保存します。

ログイン後の画面では、保存されたJWTを `/api/auth/check` に送信し、サーバー側でJWTの署名・有効期限・DB保存有無を確認します。  

ログイン後はアカウント表示画面から書籍検索画面へ遷移できます。  
書籍検索画面では書籍名またはカテゴリを条件に PostgreSQL の `products` テーブルを検索し、検索結果の書籍名をクリックすると詳細画面へ遷移します。

## 使用技術

- HTML / JavaScript
- C# / ASP.NET Core Minimal API
- PostgreSQL
- JWT
- Docker / Docker Compose

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

Docker Compose でアプリケーションとPostgreSQLを起動します。

```powershell
docker compose up --build
```

ブラウザで以下にアクセスします。

```text
http://localhost:8080/login.html
```

## 画面

- `login.html`：ログイン画面
- `account.html`：アカウント表示画面
- `search.html`：書籍検索画面
- `detail.html`：書籍詳細画面

## 検索機能

書籍検索画面で、書籍名またはカテゴリを入力して検索します。

検索条件は以下の2項目です。

- 書籍名
- カテゴリ

書籍名のみ、カテゴリのみ、または書籍名とカテゴリの両方を指定して検索できます。  
両方を指定した場合はAND条件として検索します。

```sql
WHERE (@name = '' OR name LIKE @nameLike)
  AND (@category = '' OR category LIKE @categoryLike)
```

検索結果の書籍名をクリックすると書籍詳細画面へ遷移します。

## 認証機能

ログイン成功時にJWTを発行し、ブラウザの SessionStorage に保存します。  
発行したJWTは PostgreSQL の `login_tokens` テーブルにも保存します。

ログイン後の各画面では保存されたJWTを `/api/auth/check` に送信し、サーバー側で以下を確認します。

- JWTの署名が正しいこと
- JWTの有効期限が切れていないこと
- JWTが `login_tokens` テーブルに保存されていること

ログアウト時には、`login_tokens` テーブルからJWTを削除します。

## API

### ログインAPI

```text
POST /api/login
```

入力されたユーザー名・パスワードを `users` テーブルと照合します。  
認証に成功した場合、JWTを発行して返却します。

### 認証チェックAPI

```text
POST /api/auth/check
```

画面側から送信されたJWTを検証し、ログイン状態を確認します。

### ログアウトAPI

```text
POST /api/logout
```

DBに保存されているJWTを削除します。

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