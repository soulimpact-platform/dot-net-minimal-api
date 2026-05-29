// 書籍検索、書籍詳細取得、書籍管理を行うService
public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public BookSearchResultResponse Search(
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
        var validPage = page < 1 ? 1 : page;
        var validPageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

        return _bookRepository.Search(
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

    public List<BookResponse> SearchAll(
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
        return _bookRepository.SearchAll(
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

    public List<BookResponse> GetAll()
    {
        return _bookRepository.FindAll();
    }

    public BookDetailResponse? FindById(int id)
    {
        return _bookRepository.FindById(id);
    }

    public MessageResponse Create(BookRequest request, string currentUsername)
    {
        var validationError = ValidateRequest(request);

        if (validationError is not null)
        {
            return validationError;
        }

        _bookRepository.Create(
            request.Name,
            request.Category,
            request.Author,
            request.Price,
            request.Description,
            currentUsername
        );

        return new MessageResponse(true, "書籍を追加しました。");
    }

    public MessageResponse Update(int id, BookRequest request, string currentUsername)
    {
        var book = _bookRepository.FindById(id);

        if (book is null)
        {
            return new MessageResponse(false, "書籍が見つかりません。");
        }

        var validationError = ValidateRequest(request);

        if (validationError is not null)
        {
            return validationError;
        }

        _bookRepository.Update(
            id,
            request.Name,
            request.Category,
            request.Author,
            request.Price,
            request.Description,
            currentUsername
        );

        return new MessageResponse(true, "書籍を更新しました。");
    }

    public MessageResponse Delete(int id, string currentUsername)
    {
        var book = _bookRepository.FindById(id);

        if (book is null)
        {
            return new MessageResponse(false, "書籍が見つかりません。");
        }

        _bookRepository.Delete(id, currentUsername);

        return new MessageResponse(true, "書籍を削除しました。");
    }

    private static MessageResponse? ValidateRequest(BookRequest request)
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

        return null;
    }
}
