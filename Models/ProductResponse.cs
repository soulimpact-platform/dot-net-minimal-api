// 書籍検索結果で返すレスポンス
public record ProductResponse(
    int Id,
    string Name,
    string Category,
    int Price
);