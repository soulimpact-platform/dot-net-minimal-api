// 書籍詳細表示用のレスポンス
public record BookDetailResponse(
    int Id,
    string Name,
    string Category,
    string Author,
    int Price,
    string Description
);
