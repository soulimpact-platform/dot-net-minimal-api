// 書籍情報の取得・更新処理を定義するインターフェース
public interface IBookRepository
{
    // 書籍名・カテゴリ・著者名・価格を条件に書籍を検索
    BookSearchResultResponse Search(
        string name,
        string category,
        string author,
        int? minPrice,
        int? maxPrice,
        string sortBy,
        string sortOrder,
        int page,
        int pageSize
    );

    // CSV出力用に検索条件に一致する書籍を指定件数まで取得
    List<BookResponse> SearchAll(
        string name,
        string category,
        string author,
        int? minPrice,
        int? maxPrice,
        string sortBy,
        string sortOrder,
        int limit
    );

    // 管理者向けの書籍一覧を取得
    List<BookResponse> FindAll();

    // 指定されたIDの書籍詳細を取得
    BookDetailResponse? FindById(int id);

    // 書籍を追加
    void Create(string name, string category, string author, int price, string description, string createdBy);

    // 書籍を更新
    void Update(int id, string name, string category, string author, int price, string description, string updatedBy);

    // 書籍を論理削除
    void Delete(int id, string updatedBy);
}
