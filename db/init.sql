CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username TEXT NOT NULL UNIQUE,
    password TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS products (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    category TEXT NOT NULL,
    price INTEGER NOT NULL,
    description TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS login_tokens (
    id SERIAL PRIMARY KEY,
    username TEXT NOT NULL,
    token TEXT NOT NULL UNIQUE,
    expires_at TIMESTAMPTZ NOT NULL
);

INSERT INTO users (username, password)
VALUES
    ('user01', 'password01'),
    ('user02', 'password02'),
    ('user03', 'password03'),
    ('user04', 'password04')
ON CONFLICT (username) DO NOTHING;

INSERT INTO products (id, name, category, price, description)
VALUES
    (1, '独習C#', '技術書', 3600, 'C#の基本を学べる入門書'),
    (2, 'なるほどなっとくC#入門', '技術書', 3000, 'C#の基礎を分かりやすく解説した書籍'),
    (3, '独習JavaScript 新版', '技術書', 3200, 'JavaScriptの基本を学べる入門書'),
    (4, '吾輩は猫である', '小説', 800, '夏目漱石による風刺小説'),
    (5, '坊っちゃん', '小説', 700, '夏目漱石による青春小説'),
    (6, '銀河鉄道の夜', '小説', 750, '宮沢賢治による幻想小説'),
    (7, '日本の歴史', '歴史', 1200, '日本史の流れを学べる書籍'),
    (8, '世界の歴史', '歴史', 1300, '世界史の流れを学べる書籍'),
    (9, '英単語ターゲット1900', '語学', 1100, '英単語を学習するための単語集'),
    (10, '速読英単語 必修編', '語学', 1200, '英文を読みながら単語を学べる書籍')
ON CONFLICT (id) DO NOTHING;