// 書籍検索、書籍詳細取得、書籍管理を行うService
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

    public List<ProductResponse> GetAll()
    {
        // 管理者向けの書籍一覧を取得
        return _productRepository.FindAll();
    }

    public ProductDetailResponse? FindById(int id)
    {
        // 指定されたIDの書籍詳細を取得
        return _productRepository.FindById(id);
    }

    public MessageResponse Create(ProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new MessageResponse(false, "書籍名を入力してください。");
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return new MessageResponse(false, "カテゴリを入力してください。");
        }

        if (string.IsNullOrWhiteSpace(request.Author))
        {
            return new MessageResponse(false, "著者名を入力してください。");
        }

        if (request.Price < 0)
        {
            return new MessageResponse(false, "価格は0以上で入力してください。");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return new MessageResponse(false, "説明を入力してください。");
        }

        _productRepository.Create(
            request.Name,
            request.Category,
            request.Author,
            request.Price,
            request.Description
        );

        return new MessageResponse(true, "書籍を追加しました。");
    }

    public MessageResponse Update(int id, ProductRequest request)
    {
        var product = _productRepository.FindById(id);

        if (product is null)
        {
            return new MessageResponse(false, "書籍が見つかりません。");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new MessageResponse(false, "書籍名を入力してください。");
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return new MessageResponse(false, "カテゴリを入力してください。");
        }

        if (string.IsNullOrWhiteSpace(request.Author))
        {
            return new MessageResponse(false, "著者名を入力してください。");
        }

        if (request.Price < 0)
        {
            return new MessageResponse(false, "価格は0以上で入力してください。");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return new MessageResponse(false, "説明を入力してください。");
        }

        _productRepository.Update(
            id,
            request.Name,
            request.Category,
            request.Author,
            request.Price,
            request.Description
        );

        return new MessageResponse(true, "書籍を更新しました。");
    }

    public MessageResponse Delete(int id)
    {
        var product = _productRepository.FindById(id);

        if (product is null)
        {
            return new MessageResponse(false, "書籍が見つかりません。");
        }

        if (_productRepository.HasLoanHistory(id))
        {
            return new MessageResponse(false, "貸出履歴がある書籍は削除できません。");
        }

        // 書籍を削除
        _productRepository.Delete(id);

        return new MessageResponse(true, "書籍を削除しました。");
    }
}