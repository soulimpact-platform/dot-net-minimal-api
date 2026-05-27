// 書籍追加・編集時のリクエスト
public record ProductRequest(
    string Name,
    string Category,
    string Author,
    int Price,
    string Description
);