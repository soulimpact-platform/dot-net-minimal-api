using System.Text;

// 書籍関連APIのエンドポイント定義
public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        // 書籍検索API
        // 書籍名・カテゴリ・著者名・価格を条件に検索し、検索結果一覧を返す
        app.MapGet("/api/products/search", (
            string? name,
            string? category,
            string? author,
            int? minPrice,
            int? maxPrice,
            string? sortBy,
            string? sortOrder,
            IProductService productService,
            int page = 1,
            int pageSize = 10) =>
        {
            var result = productService.Search(
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
        // 検索条件に一致する書籍をCSV形式で返す
        app.MapGet("/api/products/export-csv", (
            string? name,
            string? category,
            string? author,
            int? minPrice,
            int? maxPrice,
            string? sortBy,
            string? sortOrder,
            IProductService productService) =>
        {
            const int csvExportLimit = 10000;

            // 上限超過判定用に1件多く取得
            var products = productService.SearchAll(
                name,
                category,
                author,
                minPrice,
                maxPrice,
                sortBy,
                sortOrder,
                csvExportLimit + 1
            );

            if (products.Count > csvExportLimit)
            {
                return Results.BadRequest(new
                {
                    message = "件数が多すぎます。検索条件を絞ってください。"
                });
            }

            var csv = new StringBuilder();

            csv.AppendLine("ID,書籍名,カテゴリ,著者,価格");

            foreach (var product in products)
            {
                csv.AppendLine(
                    $"{product.Id},{EscapeCsv(product.Name)},{EscapeCsv(product.Category)},{EscapeCsv(product.Author)},{product.Price}"
                );
            }

            var shiftJis = Encoding.GetEncoding("Shift_JIS");

            return Results.File(
                shiftJis.GetBytes(csv.ToString()),
                "text/csv; charset=Shift_JIS",
                "products.csv"
            );
        })
        .RequireAuthorization();

        // 書籍詳細API
        // 指定されたIDの書籍詳細を返す
        app.MapGet("/api/products/{id:int}", (int id, IProductService productService) =>
        {
            var product = productService.FindById(id);

            if (product is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(product);
        })
        .RequireAuthorization();
    }

    // CSV出力用にリスクのある先頭文字や区切り文字を含む文字列を整形
    private static string EscapeCsv(string value)
    {
        var needsQuote = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');

        // Excel等で数式として解釈される可能性がある先頭文字をエスケープ
        if (value.Length > 0 && "=+-@\t\r".Contains(value[0]))
        {
            value = "'" + value;
            needsQuote = true;
        }

        return needsQuote ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }
}