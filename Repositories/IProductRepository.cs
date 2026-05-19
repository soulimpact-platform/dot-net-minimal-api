// 書籍情報の取得処理を定義するインターフェース
public interface IProductRepository
{
    // 書籍名・カテゴリを条件に書籍を検索
    List<ProductResponse> Search(string name, string category);

    // 指定されたIDの書籍詳細を取得
    ProductDetailResponse? FindById(int id);
}