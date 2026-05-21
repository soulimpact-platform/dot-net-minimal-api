// 書籍詳細で返すレスポンス
public record ProductDetailResponse(
    int Id,
    string Name,
    string Category,
    string Author,
    int Price,
    string Description
);