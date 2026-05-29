// 書籍追加・編集時のリクエスト
public record BookRequest(
    string Name,
    string Category,
    string Author,
    int Price,
    string Description
);
