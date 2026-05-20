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
        return;
    }

    const data = await response.json();

    // チェックAPIから返されたユーザー名を画面に表示
    document.getElementById("username").textContent = data.username;
}

document.getElementById("searchPageButton").addEventListener("click", function () {
    // 書籍検索画面へ遷移
    window.location.href = "search.html";
});

document.getElementById("logoutButton").addEventListener("click", async function () {
    // サーバー側でDBに保存しているJWTを削除
    await fetch("/api/logout", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            token: token
        })
    });

    // ブラウザ側に保存しているJWTを削除
    sessionStorage.removeItem("token");

    // ログイン画面へ戻る
    window.location.href = "login.html";
});