// ログイン時に保存したユーザー名を取得
const username = sessionStorage.getItem("username");

if (!username) {
    // ユーザー名が保存されていない場合、ログイン画面へ戻る
    window.location.href = "login.html";
}

document.getElementById("searchButton").addEventListener("click", async function () {
    // 入力された検索条件を取得
    const name = document.getElementById("name").value;
    const category = document.getElementById("category").value;
    const message = document.getElementById("message");
    const resultArea = document.getElementById("resultArea");

    message.textContent = "";
    resultArea.innerHTML = "";

    // 書籍名またはカテゴリのどちらかを入力必須とする
    if (!name && !category) {
        message.textContent = "書籍名またはカテゴリを入力してください。";
        return;
    }

    // 書籍検索APIを呼び出す
    const response = await fetch(`/api/products/search?name=${encodeURIComponent(name)}&category=${encodeURIComponent(category)}`);

    if (!response.ok) {
        message.textContent = "検索に失敗しました。";
        return;
    }

    const products = await response.json();

    if (products.length === 0) {
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