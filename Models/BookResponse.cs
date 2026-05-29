// 書籍一覧表示用のレスポンス
public record BookResponse(
    int Id,
    string Name,
    string Category,
    string Author,
    int Price
);
