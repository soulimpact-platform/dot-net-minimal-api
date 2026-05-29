# ログイン認証・書籍検索サンプルアプリ

## 概要

ログイン画面で入力されたユーザー名・パスワードを、PostgreSQL の `users` テーブルと照合します。  
パスワードはハッシュ値として保存し、ログイン時は ASP.NET Core の `PasswordHasher` を使用して検証します。

認証に成功するとJWTを発行し、ブラウザの SessionStorage に保存します。  
また発行したJWTは PostgreSQL の `login_tokens` テーブルにも保存します。

ログイン後にAPIを呼び出す際は、SessionStorage に保存したJWTを `Authorization: Bearer <token>` 形式でリクエストヘッダに設定します。
サーバー側ではJWT Bearer認証によりJWTの署名・有効期限・発行者・利用者を検証し、さらに `login_tokens` テーブルに保存されているJWTかどうかを確認します。

ログイン後はアカウント表示画面から書籍検索画面へ遷移できます。  
書籍検索画面では、書籍名・カテゴリ・著者名・価格を条件に書籍を検索できます。  
検索結果の書籍名をクリックすると、書籍詳細画面へ遷移します。

## 使用技術

- HTML / JavaScript
- C# / ASP.NET Core Minimal API
- PostgreSQL
- JWT Bearer認証
- Docker / Docker Compose

## 初期登録ユーザー

| ユーザー名 | パスワード | ロール |
|---|---|---|
| user01 | password01 | general |
| user02 | password02 | general |
| user03 | password03 | general |
| admin01 | password01 | admin |

## 初期登録書籍

| ID | 書籍名 | カテゴリ | 著者 | 価格 |
|---:|---|---|---|---:|
| 1 | 独習C# | 技術書 | 山田祥寛 | 3600 |
| 2 | なるほどなっとくC#入門 | 技術書 | 出井秀行 | 3000 |
| 3 | 独習JavaScript 新版 | 技術書 | 山田祥寛 | 3200 |
| 4 | 吾輩は猫である | 小説 | 夏目漱石 | 800 |
| 5 | 坊っちゃん | 小説 | 夏目漱石 | 700 |
| 6 | 銀河鉄道の夜 | 小説 | 宮沢賢治 | 750 |
| 7 | 日本の歴史 | 歴史 | 井上光貞 | 1200 |
| 8 | 世界の歴史 | 歴史 | 大貫良夫 | 1300 |
| 9 | 英単語ターゲット1900 | 語学 | 宮川幸久 | 1100 |
| 10 | 速読英単語 必修編 | 語学 | 風早寛 | 1200 |
| 11 | スラスラ読める JavaScript ふりがなプログラミング | 技術書 | リブロワークス | 2200 |
| 12 | スラスラ読める Python ふりがなプログラミング | 技術書 | リブロワークス | 2200 |
| 13 | こころ | 小説 | 夏目漱石 | 700 |
| 14 | 注文の多い料理店 | 小説 | 宮沢賢治 | 650 |
| 15 | 英熟語ターゲット1000 | 語学 | 花本金吾 | 1000 |
| 16 | キクタン Basic 4000 | 語学 | 一杉武史 | 1400 |
| 17 | 走れメロス | 小説 | 太宰治 | 600 |
| 18 | よくわかる日本史 | 歴史 | 石川晶康 | 1300 |
| 19 | よくわかる世界史 | 歴史 | 磯谷正行 | 1300 |
| 20 | やさしいC# | 技術書 | 高橋麻奈 | 2600 |
| 21 | SQL 第2版 ゼロからはじめるデータベース操作 | 技術書 | ミック | 2000 |
| 22 | 基礎英文法問題精講 | 語学 | 中原道喜 | 1200 |

## 起動手順

Docker Compose でアプリケーションとPostgreSQLを起動します。

```powershell
docker compose up --build
```

ブラウザで以下にアクセスします。

```text
http://localhost:8080/login.html
```

停止する場合は以下を実行します。

```powershell
docker compose down
```

DBのデータも含めて初期化したい場合は以下を実行します。

```powershell
docker compose down -v
```

## DB初期化

PostgreSQL のテーブル作成および初期データ登録は`db/init.sql` で行っています。
以下のテーブルが作成されます。

- `users`：ログインユーザー情報
- `categories`：カテゴリマスタ
- `book_authors`：書籍著者マスタ
- `books`：書籍情報
- `login_tokens`：発行済みJWT情報

## 画面

- `login.html`：ログイン画面
- `account.html`：アカウント表示画面
- `search.html`：書籍検索画面
- `detail.html`：書籍詳細画面

## 認証機能

ログイン成功時にJWTを発行し、ブラウザの SessionStorage に保存します。
発行したJWTは PostgreSQL の `login_tokens` テーブルにも保存します。

ログイン後にAPIを呼び出す際は、SessionStorage に保存したJWTを `Authorization: Bearer <token>` 形式でリクエストヘッダに設定します。

サーバー側ではJWT Bearer認証により以下を確認します。

- JWTの署名が正しいこと
- JWTの有効期限が切れていないこと
- JWTの発行者が正しいこと
- JWTの利用者が正しいこと
- JWTが `login_tokens` テーブルに保存されていること

## 検索機能

書籍検索画面では、以下の条件で検索できます。

- 書籍名
- カテゴリ
- 著者名
- 最小価格
- 最大価格

未入力の条件は検索条件から除外されます。  
複数の条件を指定した場合はAND条件として検索します。

検索結果は10件ずつx表示されます。  
書籍名・カテゴリの見出し横のボタンから昇順・降順の並び替えができます。

検索結果の書籍名をクリックすると書籍詳細画面へ遷移します。

## CSVエクスポート機能

書籍検索画面では、現在の検索条件に一致する書籍一覧をCSV形式でエクスポートできます。

## API

### ログインAPI

```text
POST /api/login
```

入力されたユーザー名・パスワードを検証します。 
認証に成功した場合、JWTを発行して返却します。

### ログアウトAPI

```text
POST /api/logout
```

Authorization ヘッダで認証されたユーザー自身のJWTをDBから削除します。

### 書籍検索API

```text
GET /api/books/search?name={書籍名}&category={カテゴリ}&author={著者名}&minPrice={最小価格}&maxPrice={最大価格}&sortBy={並び替え項目}&sortOrder={並び順}&page={ページ番号}&pageSize={表示件数}
```

検索条件に一致する書籍一覧を取得します。

### CSVエクスポートAPI

```text
GET /api/books/export-csv?name={書籍名}&category={カテゴリ}&author={著者名}&minPrice={最小価格}&maxPrice={最大価格}&sortBy={並び替え項目}&sortOrder={並び順}
```

検索条件に一致する書籍一覧をCSV形式で出力します。

### 書籍詳細API

```text
GET /api/books/{id}
```

指定されたIDの書籍情報を取得します。