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
            int page,
            int pageSize,
            IProductService productService) =>
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
        });

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
            var products = productService.SearchAll(
                name,
                category,
                author,
                minPrice,
                maxPrice,
                sortBy,
                sortOrder
            );

            var csv = new StringBuilder();

            csv.AppendLine("ID,書籍名,カテゴリ,著者,価格");

            foreach (var product in products)
            {
                csv.AppendLine(
                    $"{product.Id},{EscapeCsv(product.Name)},{EscapeCsv(product.Category)},{EscapeCsv(product.Author)},{product.Price}"
                );
            }

            return Results.File(
                Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                "products.csv"
            );
        });

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
        });
    }

    // CSV出力用にカンマ、ダブルクォーテーション、改行を含む文字列を整形
    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}