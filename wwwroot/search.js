// ログイン時に保存したJWTを取得
const token = sessionStorage.getItem("token");

if (!token) {
    // JWTが保存されていない場合、ログイン画面へ戻る
    window.location.href = "login.html";
} else {
    // JWTが有効かサーバー側で確認
    checkLogin();
}

// サーバー側でJWTの有効性を確認
async function checkLogin() {
    const response = await fetch("/api/auth/check", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            token: token
        })
    });

    if (!response.ok) {
        // JWTが無効・期限切れ・DBに存在しない場合はログイン画面へ戻る
        sessionStorage.removeItem("token");
        window.location.href = "login.html";
    }
}

document.getElementById("searchButton").addEventListener("click", async function () {
    // 入力された検索条件を取得
    const name = document.getElementById("name").value;
    const category = document.getElementById("category").value;
    const message = document.getElementById("message");
    const resultArea = document.getElementById("resultArea");

    // 前回のメッセージと検索結果をクリア
    message.textContent = "";
    resultArea.innerHTML = "";

    // 書籍検索APIを呼び出す
    const response = await fetch(`/api/products/search?name=${encodeURIComponent(name)}&category=${encodeURIComponent(category)}`);

    if (!response.ok) {
        // API呼び出しに失敗した場合
        message.textContent = "検索に失敗しました。";
        return;
    }

    const products = await response.json();

    if (products.length === 0) {
        // 検索結果が0件の場合
        message.textContent = "該当する書籍がありません。";
        return;
    }

    // 検索結果をテーブル形式で表示
    const table = document.createElement("table");
    table.className = "result-table";

    table.innerHTML = `
        <tr>
            <th>書籍名</th>
            <th>カテゴリ</th>
            <th>価格</th>
        </tr>
    `;

    products.forEach(function (product) {
        const row = document.createElement("tr");

        // 書籍名をクリックすると詳細画面へ遷移
        row.innerHTML = `
            <td><a href="detail.html?id=${product.id}">${product.name}</a></td>
            <td>${product.category}</td>
            <td>${product.price}円</td>
        `;

        table.appendChild(row);
    });

    resultArea.appendChild(table);
});

document.getElementById("backButton").addEventListener("click", function () {
    // アカウント表示画面へ戻る
    window.location.href = "account.html";
});