// 書籍検索、書籍詳細取得を行うService
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public List<ProductResponse> Search(string? name, string? category)
    {
        // 未入力の検索条件は空文字に変換
        return _productRepository.Search(
            name ?? string.Empty,
            category ?? string.Empty
        );
    }

    public ProductDetailResponse? FindById(int id)
    {
        // 指定されたIDの書籍詳細を取得
        return _productRepository.FindById(id);
    }
}