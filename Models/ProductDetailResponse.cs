// 書籍詳細で返すレスポンス
public record ProductDetailResponse(
    int Id,
    string Name,
    string Category,
    int Price,
    string Description
);