// 書籍検索、書籍詳細取得、書籍管理を行うServiceインターフェース
public interface IBookService
{
    // 書籍を検索
    BookSearchResultResponse Search(
        string? name,
        string? category,
        string? author,
        int? minPrice,
        int? maxPrice,
        string? sortBy,
        string? sortOrder,
        int page,
        int pageSize
    );

    // CSV出力用に検索条件に一致する書籍を取得
    List<BookResponse> SearchAll(
        string? name,
        string? category,
        string? author,
        int? minPrice,
        int? maxPrice,
        string? sortBy,
        string? sortOrder,
        int limit
    );

    // 管理者向けの書籍一覧を取得
    List<BookResponse> GetAll();

    // 書籍詳細を取得
    BookDetailResponse? FindById(int id);

    // 書籍を追加
    MessageResponse Create(BookRequest request, string currentUsername);

    // 書籍を更新
    MessageResponse Update(int id, BookRequest request, string currentUsername);

    // 書籍を削除
    MessageResponse Delete(int id, string currentUsername);
}
