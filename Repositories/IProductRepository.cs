// 書籍情報の取得処理を定義するインターフェース
public interface IProductRepository
{
    // 書籍名・カテゴリ・著者名・価格を条件に書籍を検索
    ProductSearchResultResponse Search(
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

    // CSV出力用に検索条件に一致する書籍を全件取得
    List<ProductResponse> SearchAll(
        string name,
        string category,
        string author,
        int? minPrice,
        int? maxPrice,
        string sortBy,
        string sortOrder
    );

    // 指定されたIDの書籍詳細を取得
    ProductDetailResponse? FindById(int id);
}