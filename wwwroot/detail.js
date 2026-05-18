// ログイン時に保存したユーザー名を取得する
const username = sessionStorage.getItem("username");

if (!username) {
    // ユーザー名が保存されていない場合、ログイン画面へ戻る
    window.location.href = "login.html";
}

// URLから書籍IDを取得
const params = new URLSearchParams(window.location.search);
const id = params.get("id");

const message = document.getElementById("message");

if (!id) {
    message.textContent = "書籍IDが指定されていません。";
} else {
    loadProductDetail(id);
}

// 詳細APIを呼び出して書籍情報を表示
async function loadProductDetail(id) {
    const response = await fetch(`/api/products/${id}`);

    if (!response.ok) {
        message.textContent = "書籍情報の取得に失敗しました。";
        return;
    }

    const product = await response.json();

    document.getElementById("name").textContent = product.name;
    document.getElementById("category").textContent = product.category;
    document.getElementById("price").textContent = `${product.price}円`;
    document.getElementById("description").textContent = product.description;
}

document.getElementById("backButton").addEventListener("click", function () {
    window.location.href = "search.html";
});