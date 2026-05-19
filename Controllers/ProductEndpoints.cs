// 書籍関連APIのエンドポイント定義
public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        // 書籍検索API
        // 書籍名・カテゴリを条件に検索し、検索結果一覧を返す
        app.MapGet("/api/products/search", (
            string? name,
            string? category,
            IProductService productService) =>
        {
            var products = productService.Search(name, category);

            return Results.Ok(products);
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
}