// ログイン時に保存したJWTを取得
const token = sessionStorage.getItem("token");

// 現在のページ番号
let currentPage = 1;

// 1ページあたりの表示件数
const pageSize = 10;

// 現在の並び替え項目
let currentSortBy = "id";

// 現在の並び順
let currentSortOrder = "asc";

if (!token) {
    // JWTが保存されていない場合、ログイン画面へ戻る
    window.location.href = "login.html";
}

function redirectToLogin() {
    // JWTを削除してログイン画面へ戻る
    sessionStorage.removeItem("token");
    window.location.href = "login.html";
}

// 検索条件を取得
function getSearchConditions() {
    const name = document.getElementById("name").value;
    const category = document.getElementById("category").value;
    const author = document.getElementById("author").value;
    const minPrice = document.getElementById("minPrice").value;
    const maxPrice = document.getElementById("maxPrice").value;

    return {
        name,
        category,
        author,
        minPrice,
        maxPrice,
        sortBy: currentSortBy,
        sortOrder: currentSortOrder
    };
}

// 検索API用のURLを作成
function createSearchUrl(page) {
    const conditions = getSearchConditions();

    const params = new URLSearchParams({
        name: conditions.name,
        category: conditions.category,
        author: conditions.author,
        sortBy: conditions.sortBy,
        sortOrder: conditions.sortOrder,
        page: page,
        pageSize: pageSize
    });

    if (conditions.minPrice) {
        params.append("minPrice", conditions.minPrice);
    }

    if (conditions.maxPrice) {
        params.append("maxPrice", conditions.maxPrice);
    }

    return `/api/products/search?${params.toString()}`;
}

// CSVエクスポートAPI用のURLを作成
function createCsvUrl() {
    const conditions = getSearchConditions();

    const params = new URLSearchParams({
        name: conditions.name,
        category: conditions.category,
        author: conditions.author,
        sortBy: conditions.sortBy,
        sortOrder: conditions.sortOrder
    });

    if (conditions.minPrice) {
        params.append("minPrice", conditions.minPrice);
    }

    if (conditions.maxPrice) {
        params.append("maxPrice", conditions.maxPrice);
    }

    return `/api/products/export-csv?${params.toString()}`;
}

document.getElementById("searchButton").addEventListener("click", async function () {
    // 検索ボタン押下時は1ページ目から表示
    currentPage = 1;

    await searchProducts();
});

// 書籍検索APIを呼び出し、検索結果を表示
async function searchProducts() {
    const message = document.getElementById("message");
    const resultArea = document.getElementById("resultArea");
    const pagingArea = document.getElementById("pagingArea");

    // 前回のメッセージ、検索結果、ページングをクリア
    message.textContent = "";
    resultArea.innerHTML = "";
    pagingArea.innerHTML = "";

    try {
        // JWTをAuthorizationヘッダに付与して書籍検索APIを呼び出し
        const response = await fetch(createSearchUrl(currentPage), {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (response.status === 401) {
            // JWTが無効・期限切れ・ログアウト済みの場合はログイン画面へ戻る
            redirectToLogin();
            return;
        }

        if (!response.ok) {
            // API呼び出しに失敗した場合
            message.textContent = "検索に失敗しました。";
            return;
        }

        const result = await response.json();
        const products = result.products;

        if (products.length === 0) {
            // 検索結果が0件の場合
            message.textContent = "該当する書籍がありません。";
            return;
        }

        // 検索結果をテーブル形式で表示
        renderProductTable(products);

        // ページングを表示
        renderPaging(result.totalCount, result.page, result.pageSize);
    } catch {
        // 通信断などでfetch自体に失敗した場合
        message.textContent = "通信エラーが発生しました。";
    }
}

// 検索結果テーブルを表示
function renderProductTable(products) {
    const resultArea = document.getElementById("resultArea");

    const table = document.createElement("table");
    table.className = "result-table";

    table.innerHTML = `
        <tr>
            <th aria-sort="${getAriaSort("name")}">
                書籍名
                <button class="sort-button" data-sort-by="name" aria-label="書籍名で並び替え">${getSortMark("name")}</button>
            </th>
            <th aria-sort="${getAriaSort("category")}">
                カテゴリ
                <button class="sort-button" data-sort-by="category" aria-label="カテゴリで並び替え">${getSortMark("category")}</button>
            </th>
            <th>著者</th>
            <th>価格</th>
        </tr>
    `;

    products.forEach(function (product) {
        const row = document.createElement("tr");

        // 書籍名リンクを作成
        const nameCell = document.createElement("td");
        const nameLink = document.createElement("a");
        nameLink.href = `detail.html?id=${encodeURIComponent(product.id)}`;
        nameLink.textContent = product.name;
        nameCell.appendChild(nameLink);
        row.appendChild(nameCell);

        // カテゴリを設定
        const categoryCell = document.createElement("td");
        categoryCell.textContent = product.category;
        row.appendChild(categoryCell);

        // 著者名を設定
        const authorCell = document.createElement("td");
        authorCell.textContent = product.author;
        row.appendChild(authorCell);

        // 価格を設定
        const priceCell = document.createElement("td");
        priceCell.textContent = `${product.price}円`;
        row.appendChild(priceCell);

        table.appendChild(row);
    });

    resultArea.appendChild(table);

    // ソートボタンのクリック処理を設定
    document.querySelectorAll(".sort-button").forEach(function (button) {
        button.addEventListener("click", async function () {
            const sortBy = button.dataset.sortBy;

            if (currentSortBy === sortBy) {
                // 同じ項目をクリックした場合は昇順と降順を切り替え
                currentSortOrder = currentSortOrder === "asc" ? "desc" : "asc";
            } else {
                // 別の項目をクリックした場合は昇順から開始
                currentSortBy = sortBy;
                currentSortOrder = "asc";
            }

            // 並び替え時は1ページ目から表示
            currentPage = 1;

            await searchProducts();
        });
    });
}

// 現在の並び順に応じたソート表示を取得
function getSortMark(sortBy) {
    if (currentSortBy !== sortBy) {
        return "↕";
    }

    return currentSortOrder === "asc" ? "▲" : "▼";
}

// 現在の並び順に応じたaria-sort属性値を取得
function getAriaSort(sortBy) {
    if (currentSortBy !== sortBy) {
        return "none";
    }

    return currentSortOrder === "asc" ? "ascending" : "descending";
}

// ページングを表示
function renderPaging(totalCount, page, pageSize) {
    const pagingArea = document.getElementById("pagingArea");

    const totalPages = Math.ceil(totalCount / pageSize);

    if (totalPages <= 1) {
        return;
    }

    const previousButton = document.createElement("button");
    previousButton.textContent = "前へ";
    previousButton.className = "paging-button";
    previousButton.disabled = page <= 1;

    previousButton.addEventListener("click", async function () {
        currentPage--;
        await searchProducts();
    });

    const pageText = document.createElement("span");
    pageText.textContent = ` ${page} / ${totalPages} ページ `;

    const nextButton = document.createElement("button");
    nextButton.textContent = "次へ";
    nextButton.className = "paging-button";
    nextButton.disabled = page >= totalPages;

    nextButton.addEventListener("click", async function () {
        currentPage++;
        await searchProducts();
    });

    pagingArea.appendChild(previousButton);
    pagingArea.appendChild(pageText);
    pagingArea.appendChild(nextButton);
}

document.getElementById("csvButton").addEventListener("click", async function () {
    const message = document.getElementById("message");

    // 前回のメッセージをクリア
    message.textContent = "";

    try {
        // JWTをAuthorizationヘッダに付与してCSVエクスポートAPIを呼び出し
        const response = await fetch(createCsvUrl(), {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (response.status === 401) {
            // JWTが無効・期限切れ・ログアウト済みの場合はログイン画面へ戻る
            redirectToLogin();
            return;
        }

        if (!response.ok) {
            const error = await response.json();

            // CSV出力に失敗した場合
            message.textContent = error.message ?? "CSVエクスポートに失敗しました。";
            return;
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);

        const link = document.createElement("a");
        link.href = url;
        link.download = "products.csv";
        link.click();

        window.URL.revokeObjectURL(url);
    } catch {
        // 通信断などでfetch自体に失敗した場合
        message.textContent = "通信エラーが発生しました。";
    }
});

document.getElementById("backButton").addEventListener("click", function () {
    // アカウント表示画面へ戻る
    window.location.href = "account.html";
});