// ログイン時に保存したJWTを取得
const token = sessionStorage.getItem("token");

if (!token) {
    // JWTが保存されていない場合、ログイン画面へ戻る
    window.location.href = "login.html";
} else {
    // JWTが有効かサーバー側で確認
    checkLogin();
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
        // JWTが無効、期限切れ、またはDBに存在しない場合はログイン画面へ戻る
        sessionStorage.removeItem("token");
        window.location.href = "login.html";
    }
}

// 詳細APIを呼び出して書籍情報を表示
async function loadProductDetail(id) {
    const response = await fetch(`/api/products/${id}`);

    if (!response.ok) {
        // 詳細情報の取得に失敗した場合
        message.textContent = "書籍情報の取得に失敗しました。";
        return;
    }

    const product = await response.json();

    // 取得した書籍情報を画面に表示
    document.getElementById("name").textContent = product.name;
    document.getElementById("category").textContent = product.category;
    document.getElementById("price").textContent = `${product.price}円`;
    document.getElementById("description").textContent = product.description;
}

document.getElementById("backButton").addEventListener("click", function () {
    // 書籍検索画面へ戻る
    window.location.href = "search.html";
});