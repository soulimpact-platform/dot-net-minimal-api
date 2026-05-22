CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    role TEXT NOT NULL CHECK (role IN ('general', 'admin'))
);

CREATE TABLE IF NOT EXISTS categories (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS authors (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS products (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    category_id INTEGER NOT NULL,
    author_id INTEGER NOT NULL,
    price INTEGER NOT NULL,
    description TEXT NOT NULL,
    FOREIGN KEY (category_id) REFERENCES categories(id),
    FOREIGN KEY (author_id) REFERENCES authors(id)
);

CREATE TABLE IF NOT EXISTS login_tokens (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    token TEXT NOT NULL UNIQUE,
    expires_at TIMESTAMPTZ NOT NULL,
    FOREIGN KEY (user_id) REFERENCES users(id)
);

INSERT INTO users (id, username, password_hash, role) OVERRIDING SYSTEM VALUE
VALUES
    (1, 'user01', 'AQAAAAIAAYagAAAAEC48kKaMuP6zY/RXqTU6PbZTNqUk4r58r10emPQcZhVrhw6Whqo+nQ7OF1/ke58qJw==', 'general'),
    (2, 'user02', 'AQAAAAIAAYagAAAAEEA7jXawoH6RZDqQF1OeEyp7XaaqyhU7l4SuHsjXKdcGfqblXP4rqwYiXuYma2JNDw==', 'general'),
    (3, 'user03', 'AQAAAAIAAYagAAAAEANvc67hZn4I5Q6DFvfML14mEYXrNFLnQHe6NsnLvsDyNMZUMUUCJPd5oT3THS9GRA==', 'general'),
    (4, 'admin01', 'AQAAAAIAAYagAAAAEC48kKaMuP6zY/RXqTU6PbZTNqUk4r58r10emPQcZhVrhw6Whqo+nQ7OF1/ke58qJw==', 'admin')
ON CONFLICT (username) DO NOTHING;

INSERT INTO categories (id, name)
VALUES
    (1, '技術書'),
    (2, '小説'),
    (3, '歴史'),
    (4, '語学')
ON CONFLICT (id) DO NOTHING;

INSERT INTO authors (id, name)
VALUES
    (1, '山田祥寛'),
    (2, '出井秀行'),
    (3, '外村将大'),
    (4, '夏目漱石'),
    (5, '宮沢賢治'),
    (6, '井上光貞'),
    (7, '大貫良夫'),
    (8, '宮川幸久'),
    (9, '風早寛'),
    (10, 'リブロワークス'),
    (11, '花本金吾'),
    (12, '一杉武史'),
    (13, '太宰治'),
    (14, '石川晶康'),
    (15, '磯谷正行'),
    (16, '高橋麻奈'),
    (17, 'ミック'),
    (18, '中原道喜')
ON CONFLICT (id) DO NOTHING;

INSERT INTO products (id, name, category_id, author_id, price, description)
VALUES
    (1, '独習C#', 1, 1, 3600, 'C#の基本を学べる入門書'),
    (2, 'なるほどなっとくC#入門', 1, 2, 3000, 'C#の基礎を分かりやすく解説した書籍'),
    (3, '独習JavaScript 新版', 1, 1, 3200, 'JavaScriptの基本を学べる入門書'),
    (4, '吾輩は猫である', 2, 4, 800, '夏目漱石による風刺小説'),
    (5, '坊っちゃん', 2, 4, 700, '夏目漱石による青春小説'),
    (6, '銀河鉄道の夜', 2, 5, 750, '宮沢賢治による幻想小説'),
    (7, '日本の歴史', 3, 6, 1200, '日本史の流れを学べる書籍'),
    (8, '世界の歴史', 3, 7, 1300, '世界史の流れを学べる書籍'),
    (9, '英単語ターゲット1900', 4, 8, 1100, '英単語を学習するための単語集'),
    (10, '速読英単語 必修編', 4, 9, 1200, '英文を読みながら単語を学べる書籍'),
    (11, 'スラスラ読める JavaScript ふりがなプログラミング', 1, 10, 2200, 'JavaScriptを初学者向けに解説した書籍'),
    (12, 'スラスラ読める Python ふりがなプログラミング', 1, 10, 2200, 'Pythonを初学者向けに解説した書籍'),
    (13, 'こころ', 2, 4, 700, '夏目漱石による長編小説'),
    (14, '注文の多い料理店', 2, 5, 650, '宮沢賢治による童話集'),
    (15, '英熟語ターゲット1000', 4, 11, 1000, '英熟語を学習するための参考書'),
    (16, 'キクタン Basic 4000', 4, 12, 1400, '英単語をリズムで学習するための単語集'),
    (17, '走れメロス', 2, 13, 600, '太宰治による短編小説'),
    (18, 'よくわかる日本史', 3, 14, 1300, '日本史の基本を学べる書籍'),
    (19, 'よくわかる世界史', 3, 15, 1300, '世界史の基本を学べる書籍'),
    (20, 'やさしいC#', 1, 16, 2600, 'C#の基本を学べる読みやすい入門書'),
    (21, 'SQL 第2版 ゼロからはじめるデータベース操作', 1, 17, 2000, 'SQLの基礎を学べる入門書'),
    (22, '基礎英文法問題精講', 4, 18, 1200, '英文法を学習するための参考書')
ON CONFLICT (id) DO NOTHING;