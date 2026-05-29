// ログイン時に保存したJWTを取得
const token = sessionStorage.getItem("token");

// URLから書籍IDを取得
const params = new URLSearchParams(window.location.search);
const id = params.get("id");

const message = document.getElementById("message");

if (!token) {
    // JWTが保存されていない場合、ログイン画面へ戻る
    window.location.href = "login.html";
}

// JWTを削除してログイン画面へ戻る
function redirectToLogin() {
    sessionStorage.removeItem("token");
    window.location.href = "login.html";
}

// 詳細APIを呼び出して書籍情報を表示
async function loadBookDetail(id) {
    try {
        // JWTをAuthorizationヘッダに付与して書籍詳細APIを呼び出し
        const response = await fetch(`/api/books/${id}`, {
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
            message.textContent = "書籍詳細の取得に失敗しました。";
            return;
        }

        const book = await response.json();

        // 取得した書籍情報を画面に表示
        document.getElementById("name").textContent = book.name;
        document.getElementById("category").textContent = book.category;
        document.getElementById("author").textContent = book.author;
        document.getElementById("price").textContent = `${book.price}円`;
        document.getElementById("description").textContent = book.description;
    } catch {
        // 通信断などでfetch自体に失敗した場合
        message.textContent = "通信エラーが発生しました。";
    }
}

if (!id) {
    // 書籍IDが取得できない場合はエラーメッセージを表示
    message.textContent = "書籍IDが取得できません。";
} else {
    // 書籍IDをもとに詳細情報を取得
    loadBookDetail(id);
}

document.getElementById("backButton").addEventListener("click", function () {
    // 書籍検索画面へ戻る
    window.location.href = "search.html";
});
