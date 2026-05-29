CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username TEXT NOT NULL,
    password_hash TEXT NOT NULL,
    role TEXT NOT NULL CHECK (role IN ('general', 'admin')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL,
    created_by TEXT NOT NULL DEFAULT 'system',
    updated_by TEXT NOT NULL DEFAULT 'system'
);

CREATE TABLE IF NOT EXISTS categories (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL,
    created_by TEXT NOT NULL DEFAULT 'system',
    updated_by TEXT NOT NULL DEFAULT 'system'
);

CREATE TABLE IF NOT EXISTS book_authors (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL,
    created_by TEXT NOT NULL DEFAULT 'system',
    updated_by TEXT NOT NULL DEFAULT 'system'
);

CREATE TABLE IF NOT EXISTS books (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    category_id INTEGER NOT NULL,
    author_id INTEGER NOT NULL,
    price INTEGER NOT NULL,
    description TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL,
    created_by TEXT NOT NULL DEFAULT 'system',
    updated_by TEXT NOT NULL DEFAULT 'system',
    CONSTRAINT fk_books_category
        FOREIGN KEY (category_id)
        REFERENCES categories(id),
    CONSTRAINT fk_books_author
        FOREIGN KEY (author_id)
        REFERENCES book_authors(id)
);

CREATE TABLE IF NOT EXISTS login_tokens (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    token TEXT NOT NULL UNIQUE,
    expires_at TIMESTAMPTZ NOT NULL,
    CONSTRAINT fk_login_tokens_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
);

CREATE TABLE IF NOT EXISTS book_loans (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    book_id INTEGER NOT NULL,
    borrowed_at TIMESTAMPTZ NOT NULL,
    returned_at TIMESTAMPTZ NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL,
    created_by TEXT NOT NULL DEFAULT 'system',
    updated_by TEXT NOT NULL DEFAULT 'system',
    CONSTRAINT fk_book_loans_user
        FOREIGN KEY (user_id)
        REFERENCES users(id),
    CONSTRAINT fk_book_loans_book
        FOREIGN KEY (book_id)
        REFERENCES books(id)
);

CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_users_updated_at ON users;
CREATE TRIGGER trg_users_updated_at
BEFORE UPDATE ON users
FOR EACH ROW
EXECUTE FUNCTION set_updated_at();

DROP TRIGGER IF EXISTS trg_categories_updated_at ON categories;
CREATE TRIGGER trg_categories_updated_at
BEFORE UPDATE ON categories
FOR EACH ROW
EXECUTE FUNCTION set_updated_at();

DROP TRIGGER IF EXISTS trg_book_authors_updated_at ON book_authors;
CREATE TRIGGER trg_book_authors_updated_at
BEFORE UPDATE ON book_authors
FOR EACH ROW
EXECUTE FUNCTION set_updated_at();

DROP TRIGGER IF EXISTS trg_books_updated_at ON books;
CREATE TRIGGER trg_books_updated_at
BEFORE UPDATE ON books
FOR EACH ROW
EXECUTE FUNCTION set_updated_at();

DROP TRIGGER IF EXISTS trg_book_loans_updated_at ON book_loans;
CREATE TRIGGER trg_book_loans_updated_at
BEFORE UPDATE ON book_loans
FOR EACH ROW
EXECUTE FUNCTION set_updated_at();

CREATE UNIQUE INDEX IF NOT EXISTS ux_users_username_active
ON users(username)
WHERE deleted_at IS NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_categories_name_active
ON categories(name)
WHERE deleted_at IS NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_book_authors_name_active
ON book_authors(name)
WHERE deleted_at IS NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_book_loans_active_book
ON book_loans(book_id)
WHERE returned_at IS NULL
  AND deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_books_category_id ON books(category_id);
CREATE INDEX IF NOT EXISTS ix_books_author_id ON books(author_id);

CREATE INDEX IF NOT EXISTS ix_login_tokens_user_id ON login_tokens(user_id);
CREATE INDEX IF NOT EXISTS ix_login_tokens_expires_at ON login_tokens(expires_at);

CREATE INDEX IF NOT EXISTS ix_book_loans_user_id ON book_loans(user_id);
CREATE INDEX IF NOT EXISTS ix_book_loans_book_id ON book_loans(book_id);
CREATE INDEX IF NOT EXISTS ix_book_loans_borrowed_at ON book_loans(borrowed_at);
CREATE INDEX IF NOT EXISTS ix_book_loans_returned_at ON book_loans(returned_at);

CREATE INDEX IF NOT EXISTS ix_users_active ON users(id)
WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_books_active ON books(id)
WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_categories_active ON categories(id)
WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_book_authors_active ON book_authors(id)
WHERE deleted_at IS NULL;

INSERT INTO users (id, username, password_hash, role, created_by, updated_by)
VALUES
    (1, 'user01', 'AQAAAAIAAYagAAAAEC48kKaMuP6zY/RXqTU6PbZTNqUk4r58r10emPQcZhVrhw6Whqo+nQ7OF1/ke58qJw==', 'general', 'system', 'system'),
    (2, 'user02', 'AQAAAAIAAYagAAAAEEA7jXawoH6RZDqQF1OeEyp7XaaqyhU7l4SuHsjXKdcGfqblXP4rqwYiXuYma2JNDw==', 'general', 'system', 'system'),
    (3, 'user03', 'AQAAAAIAAYagAAAAEANvc67hZn4I5Q6DFvfML14mEYXrNFLnQHe6NsnLvsDyNMZUMUUCJPd5oT3THS9GRA==', 'general', 'system', 'system'),
    (4, 'admin01', 'AQAAAAIAAYagAAAAEC48kKaMuP6zY/RXqTU6PbZTNqUk4r58r10emPQcZhVrhw6Whqo+nQ7OF1/ke58qJw==', 'admin', 'system', 'system')
ON CONFLICT (id) DO NOTHING;

SELECT setval(
    pg_get_serial_sequence('users', 'id'),
    COALESCE((SELECT MAX(id) FROM users), 1),
    true
);

INSERT INTO categories (id, name, created_by, updated_by)
VALUES
    (1, '技術書', 'system', 'system'),
    (2, '小説', 'system', 'system'),
    (3, '歴史', 'system', 'system'),
    (4, '語学', 'system', 'system')
ON CONFLICT (id) DO NOTHING;

SELECT setval(
    pg_get_serial_sequence('categories', 'id'),
    COALESCE((SELECT MAX(id) FROM categories), 1),
    true
);

INSERT INTO book_authors (id, name, created_by, updated_by)
VALUES
    (1, '山田祥寛', 'system', 'system'),
    (2, '出井秀行', 'system', 'system'),
    (3, '外村将大', 'system', 'system'),
    (4, '夏目漱石', 'system', 'system'),
    (5, '宮沢賢治', 'system', 'system'),
    (6, '井上光貞', 'system', 'system'),
    (7, '大貫良夫', 'system', 'system'),
    (8, '宮川幸久', 'system', 'system'),
    (9, '風早寛', 'system', 'system'),
    (10, 'リブロワークス', 'system', 'system'),
    (11, '花本金吾', 'system', 'system'),
    (12, '一杉武史', 'system', 'system'),
    (13, '太宰治', 'system', 'system'),
    (14, '石川晶康', 'system', 'system'),
    (15, '磯谷正行', 'system', 'system'),
    (16, '高橋麻奈', 'system', 'system'),
    (17, 'ミック', 'system', 'system'),
    (18, '中原道喜', 'system', 'system')
ON CONFLICT (id) DO NOTHING;

SELECT setval(
    pg_get_serial_sequence('book_authors', 'id'),
    COALESCE((SELECT MAX(id) FROM book_authors), 1),
    true
);

INSERT INTO books (id, name, category_id, author_id, price, description, created_by, updated_by)
VALUES
    (1, '独習C#', 1, 1, 3600, 'C#の基本を学べる入門書', 'system', 'system'),
    (2, 'なるほどなっとくC#入門', 1, 2, 3000, 'C#の基礎を分かりやすく解説した書籍', 'system', 'system'),
    (3, '独習JavaScript 新版', 1, 1, 3200, 'JavaScriptの基本を学べる入門書', 'system', 'system'),
    (4, '吾輩は猫である', 2, 4, 800, '夏目漱石による風刺小説', 'system', 'system'),
    (5, '坊っちゃん', 2, 4, 700, '夏目漱石による青春小説', 'system', 'system'),
    (6, '銀河鉄道の夜', 2, 5, 750, '宮沢賢治による幻想小説', 'system', 'system'),
    (7, '日本の歴史', 3, 6, 1200, '日本史の流れを学べる書籍', 'system', 'system'),
    (8, '世界の歴史', 3, 7, 1300, '世界史の流れを学べる書籍', 'system', 'system'),
    (9, '英単語ターゲット1900', 4, 8, 1100, '英単語を学習するための単語集', 'system', 'system'),
    (10, '速読英単語 必修編', 4, 9, 1200, '英文を読みながら単語を学べる書籍', 'system', 'system'),
    (11, 'スラスラ読める JavaScript ふりがなプログラミング', 1, 10, 2200, 'JavaScriptを初学者向けに解説した書籍', 'system', 'system'),
    (12, 'スラスラ読める Python ふりがなプログラミング', 1, 10, 2200, 'Pythonを初学者向けに解説した書籍', 'system', 'system'),
    (13, 'こころ', 2, 4, 700, '夏目漱石による長編小説', 'system', 'system'),
    (14, '注文の多い料理店', 2, 5, 650, '宮沢賢治による童話集', 'system', 'system'),
    (15, '英熟語ターゲット1000', 4, 11, 1000, '英熟語を学習するための参考書', 'system', 'system'),
    (16, 'キクタン Basic 4000', 4, 12, 1400, '英単語をリズムで学習するための単語集', 'system', 'system'),
    (17, '走れメロス', 2, 13, 600, '太宰治による短編小説', 'system', 'system'),
    (18, 'よくわかる日本史', 3, 14, 1300, '日本史の基本を学べる書籍', 'system', 'system'),
    (19, 'よくわかる世界史', 3, 15, 1300, '世界史の基本を学べる書籍', 'system', 'system'),
    (20, 'やさしいC#', 1, 16, 2600, 'C#の基本を学べる読みやすい入門書', 'system', 'system'),
    (21, 'SQL 第2版 ゼロからはじめるデータベース操作', 1, 17, 2000, 'SQLの基礎を学べる入門書', 'system', 'system'),
    (22, '基礎英文法問題精講', 4, 18, 1200, '英文法を学習するための参考書', 'system', 'system')
ON CONFLICT (id) DO NOTHING;

SELECT setval(
    pg_get_serial_sequence('books', 'id'),
    COALESCE((SELECT MAX(id) FROM books), 1),
    true
);