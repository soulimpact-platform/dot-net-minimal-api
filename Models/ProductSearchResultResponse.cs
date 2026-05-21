// 書籍検索結果一覧で返すレスポンス
public record ProductSearchResultResponse(
    List<ProductResponse> Products,
    int TotalCount,
    int Page,
    int PageSize
);