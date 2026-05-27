// 書籍情報の取得・更新処理を定義するインターフェース
public interface IProductService
{
    // 書籍名・カテゴリ・著者名・価格を条件に書籍を検索
    ProductSearchResultResponse Search(
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

    // CSV出力用に検索条件に一致する書籍を指定件数まで取得
    List<ProductResponse> SearchAll(
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
    List<ProductResponse> GetAll();

    // 指定されたIDの書籍詳細を取得
    ProductDetailResponse? FindById(int id);

    // 書籍を追加
    MessageResponse Create(ProductRequest request);

    // 書籍を更新
    MessageResponse Update(int id, ProductRequest request);

    // 書籍を削除
    MessageResponse Delete(int id);
}