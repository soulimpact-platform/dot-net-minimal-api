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

// 価格の下限と上限の入力内容を確認
function validatePriceRange(message) {
    const conditions = getSearchConditions();

    if (conditions.minPrice !== "" &&
        conditions.maxPrice !== "" &&
        Number(conditions.minPrice) > Number(conditions.maxPrice)) {
        message.textContent = "価格の下限は上限以下で指定してください。";
        return false;
    }

    return true;
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

    if (conditions.minPrice !== "") {
        params.append("minPrice", conditions.minPrice);
    }

    if (conditions.maxPrice !== "") {
        params.append("maxPrice", conditions.maxPrice);
    }

    return `/api/books/search?${params.toString()}`;
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

    if (conditions.minPrice !== "") {
        params.append("minPrice", conditions.minPrice);
    }

    if (conditions.maxPrice !== "") {
        params.append("maxPrice", conditions.maxPrice);
    }

    return `/api/books/export-csv?${params.toString()}`;
}

document.getElementById("searchButton").addEventListener("click", async function () {
    // 検索ボタン押下時は1ページ目から表示
    currentPage = 1;

    await searchBooks();
});

// 書籍検索APIを呼び出し、検索結果を表示
async function searchBooks() {
    const message = document.getElementById("message");
    const resultArea = document.getElementById("resultArea");
    const pagingArea = document.getElementById("pagingArea");

    // 前回のメッセージ、検索結果、ページングをクリア
    message.textContent = "";
    resultArea.innerHTML = "";
    pagingArea.innerHTML = "";

    if (!validatePriceRange(message)) {
        return;
    }

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
        const books = result.books;

        if (books.length === 0) {
            // 検索結果が0件の場合
            message.textContent = "該当する書籍がありません。";
            return;
        }

        // 検索結果をテーブル形式で表示
        renderBookTable(books);

        // ページングを表示
        renderPaging(result.totalCount, result.page, result.pageSize);
    } catch {
        // 通信断などでfetch自体に失敗した場合
        message.textContent = "通信エラーが発生しました。";
    }
}

// 検索結果テーブルを表示
function renderBookTable(books) {
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

    books.forEach(function (book) {
        const row = document.createElement("tr");

        // 書籍名リンクを作成
        const nameCell = document.createElement("td");
        const nameLink = document.createElement("a");
        nameLink.href = `detail.html?id=${encodeURIComponent(book.id)}`;
        nameLink.textContent = book.name;
        nameCell.appendChild(nameLink);
        row.appendChild(nameCell);

        // カテゴリを設定
        const categoryCell = document.createElement("td");
        categoryCell.textContent = book.category;
        row.appendChild(categoryCell);

        // 著者名を設定
        const authorCell = document.createElement("td");
        authorCell.textContent = book.author;
        row.appendChild(authorCell);

        // 価格を設定
        const priceCell = document.createElement("td");
        priceCell.textContent = `${book.price}円`;
        row.appendChild(priceCell);

        table.appendChild(row);
    });

    resultArea.appendChild(table);

    // 検索結果テーブル内のソートボタンにクリック処理を設定
    table.querySelectorAll(".sort-button").forEach(function (button) {
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

            await searchBooks();
        });
    });
}

function getSortMark(sortBy) {
    if (currentSortBy !== sortBy) {
        return "↕";
    }

    return currentSortOrder === "asc" ? "▲" : "▼";
}

function getAriaSort(sortBy) {
    if (currentSortBy !== sortBy) {
        return "none";
    }

    return currentSortOrder === "asc" ? "ascending" : "descending";
}

function renderPaging(totalCount, page, pageSize) {
    const pagingArea = document.getElementById("pagingArea");
    const totalPages = Math.ceil(totalCount / pageSize);

    if (totalPages <= 1) {
        return;
    }

    const prevButton = document.createElement("button");
    prevButton.textContent = "前へ";
    prevButton.className = "paging-button";
    prevButton.disabled = page <= 1;
    prevButton.addEventListener("click", async function () {
        currentPage -= 1;
        await searchBooks();
    });

    const nextButton = document.createElement("button");
    nextButton.textContent = "次へ";
    nextButton.className = "paging-button";
    nextButton.disabled = page >= totalPages;
    nextButton.addEventListener("click", async function () {
        currentPage += 1;
        await searchBooks();
    });

    const pageInfo = document.createElement("span");
    pageInfo.textContent = `${page} / ${totalPages} ページ`;

    pagingArea.appendChild(prevButton);
    pagingArea.appendChild(pageInfo);
    pagingArea.appendChild(nextButton);
}

document.getElementById("csvButton").addEventListener("click", async function () {
    const message = document.getElementById("message");
    message.textContent = "";

    if (!validatePriceRange(message)) {
        return;
    }

    try {
        const response = await fetch(createCsvUrl(), {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (response.status === 401) {
            redirectToLogin();
            return;
        }

        if (!response.ok) {
            message.textContent = "CSV出力に失敗しました。";
            return;
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement("a");
        link.href = url;
        link.download = "books.csv";
        link.click();
        window.URL.revokeObjectURL(url);
    } catch {
        message.textContent = "通信エラーが発生しました。";
    }
});

document.getElementById("backButton").addEventListener("click", function () {
    window.location.href = "account.html";
});
