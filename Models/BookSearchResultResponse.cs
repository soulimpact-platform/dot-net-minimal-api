// 書籍検索結果表示用のレスポンス
public record BookSearchResultResponse(
    List<BookResponse> Books,
    int TotalCount,
    int Page,
    int PageSize
);
