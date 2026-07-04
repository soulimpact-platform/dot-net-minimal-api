using System.Text;

// 書籍関連APIのエンドポイント定義
public static class BookEndpoints
{
    public static void MapBookEndpoints(this WebApplication app)
    {
        // 書籍検索API
        app.MapGet("/api/books/search", (
            string? name,
            string? category,
            string? author,
            int? minPrice,
            int? maxPrice,
            string? sortBy,
            string? sortOrder,
            IBookService bookService,
            int page = 1,
            int pageSize = 10) =>
        {
            var result = bookService.Search(
                name,
                category,
                author,
                minPrice,
                maxPrice,
                sortBy,
                sortOrder,
                page,
                pageSize
            );

            return Results.Ok(result);
        })
        .RequireAuthorization();

        // 書籍CSVエクスポートAPI
        app.MapGet("/api/books/export-csv", (
            string? name,
            string? category,
            string? author,
            int? minPrice,
            int? maxPrice,
            string? sortBy,
            string? sortOrder,
            IBookService bookService) =>
        {
            const int csvExportLimit = 10000;

            var books = bookService.SearchAll(
                name,
                category,
                author,
                minPrice,
                maxPrice,
                sortBy,
                sortOrder,
                csvExportLimit + 1
            );

            if (books.Count > csvExportLimit)
            {
                return Results.BadRequest(new
                {
                    message = "件数が多すぎます。検索条件を絞ってください。"
                });
            }

            var csv = new StringBuilder();
            csv.AppendLine("ID,書籍名,カテゴリ,著者,価格");

            foreach (var book in books)
            {
                csv.AppendLine(
                    $"{book.Id},{EscapeCsv(book.Name)},{EscapeCsv(book.Category)},{EscapeCsv(book.Author)},{book.Price}"
                );
            }

            var shiftJis = Encoding.GetEncoding("Shift_JIS");

            return Results.File(
                shiftJis.GetBytes(csv.ToString()),
                "text/csv; charset=Shift_JIS",
                "books.csv"
            );
        })
        .RequireAuthorization();

        // 書籍詳細API
        app.MapGet("/api/books/{id:int}", (int id, IBookService bookService) =>
        {
            var book = bookService.FindById(id);

            if (book is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(book);
        })
        .RequireAuthorization();

        // 管理者向け書籍一覧取得API
        // TODO: 書籍件数が増えた場合はページング対応を検討する
        app.MapGet("/api/admin/books", (IBookService bookService) =>
        {
            var books = bookService.GetAll();

            return Results.Ok(books);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"));

        // 管理者向け書籍詳細取得API
        app.MapGet("/api/admin/books/{id:int}", (int id, IBookService bookService) =>
        {
            var book = bookService.FindById(id);

            if (book is null)
            {
                return Results.NotFound(new MessageResponse(false, "書籍が見つかりません。"));
            }

            return Results.Ok(book);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"));

        // 管理者向け書籍追加API
        app.MapPost("/api/admin/books", (BookRequest request, HttpContext context, IBookService bookService) =>
        {
            var currentUsername = GetUsername(context);

            if (string.IsNullOrEmpty(currentUsername))
            {
                return Results.Unauthorized();
            }

            var result = bookService.Create(request, currentUsername);

            if (!result.Success)
            {
                return Results.BadRequest(result);
            }

            return Results.Ok(result);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"));

        // 管理者向け書籍更新API
        app.MapPut("/api/admin/books/{id:int}", (int id, BookRequest request, HttpContext context, IBookService bookService) =>
        {
            var currentUsername = GetUsername(context);

            if (string.IsNullOrEmpty(currentUsername))
            {
                return Results.Unauthorized();
            }

            var result = bookService.Update(id, request, currentUsername);

            if (!result.Success)
            {
                return Results.BadRequest(result);
            }

            return Results.Ok(result);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"));

        // 管理者向け書籍削除API
        app.MapDelete("/api/admin/books/{id:int}", (int id, HttpContext context, IBookService bookService) =>
        {
            var currentUsername = GetUsername(context);

            if (string.IsNullOrEmpty(currentUsername))
            {
                return Results.Unauthorized();
            }

            var result = bookService.Delete(id, currentUsername);

            if (!result.Success)
            {
                return Results.BadRequest(result);
            }

            return Results.Ok(result);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"));
    }

    private static string? GetUsername(HttpContext context)
    {
        return context.User.FindFirst("username")?.Value;
    }

    private static string EscapeCsv(string value)
    {
        var needsQuote = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');

        if (value.Length > 0 && "=+-@\t\r".Contains(value[0]))
        {
            value = "'" + value;
            needsQuote = true;
        }

        return needsQuote ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }
}
