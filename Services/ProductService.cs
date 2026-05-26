// 書籍検索、書籍詳細取得を行うService
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public ProductSearchResultResponse Search(
        string? name,
        string? category,
        string? author,
        int? minPrice,
        int? maxPrice,
        string? sortBy,
        string? sortOrder,
        int page,
        int pageSize
    )
    {
        // ページ番号が不正な場合は1ページ目に補正
        var validPage = page < 1 ? 1 : page;

        // 1ページ件数が不正な場合は10件、上限超過時は100件に補正
        var validPageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

        return _productRepository.Search(
            name ?? string.Empty,
            category ?? string.Empty,
            author ?? string.Empty,
            minPrice,
            maxPrice,
            sortBy ?? "id",
            sortOrder ?? "asc",
            validPage,
            validPageSize
        );
    }

    public List<ProductResponse> SearchAll(
        string? name,
        string? category,
        string? author,
        int? minPrice,
        int? maxPrice,
        string? sortBy,
        string? sortOrder,
        int limit
    )
    {
        return _productRepository.SearchAll(
            name ?? string.Empty,
            category ?? string.Empty,
            author ?? string.Empty,
            minPrice,
            maxPrice,
            sortBy ?? "id",
            sortOrder ?? "asc",
            limit
        );
    }

    public ProductDetailResponse? FindById(int id)
    {
        // 指定されたIDの書籍詳細を取得
        return _productRepository.FindById(id);
    }
}