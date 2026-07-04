// ログイン時に保存したJWTを取得
const token = sessionStorage.getItem("token");

if (!token) {
    // JWTが保存されていない場合、ログイン画面へ戻る
    window.location.href = "login.html";
} else {
    // JWTのpayloadからユーザー情報を取得
    const payload = decodeJwtPayload(token);

    // JWTから取得したユーザー名を画面に表示
    document.getElementById("username").textContent = payload.username;

    // 管理者ユーザの場合のみ管理者メニューを表示
    if (payload.role === "admin") {
        document.getElementById("adminMenu").style.display = "block";
    }
}

// JWTを削除してログイン画面へ戻る
function redirectToLogin() {
    sessionStorage.removeItem("token");
    window.location.href = "login.html";
}

// JWTのpayload部分をデコード
function decodeJwtPayload(token) {
    const payload = token.split(".")[1];
    const base64 = payload.replace(/-/g, "+").replace(/_/g, "/");
    const json = decodeURIComponent(
        atob(base64)
            .split("")
            .map(function (char) {
                return "%" + ("00" + char.charCodeAt(0).toString(16)).slice(-2);
            })
            .join("")
    );

    return JSON.parse(json);
}

document.getElementById("searchPageButton").addEventListener("click", function () {
    // 書籍検索画面へ遷移
    window.location.href = "search.html";
});

document.getElementById("userManageButton").addEventListener("click", function () {
    // ユーザ管理画面へ遷移
    window.location.href = "user-list.html";
});

document.getElementById("bookManageButton").addEventListener("click", function () {
    // 書籍管理画面へ遷移
    window.location.href = "book-list.html";
});

document.getElementById("logoutButton").addEventListener("click", async function () {
    // JWTをAuthorizationヘッダに付与してログアウトAPIを呼び出し
    await fetch("/api/logout", {
        method: "POST",
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });

    // ブラウザ側に保存しているJWTを削除
    sessionStorage.removeItem("token");

    // ログイン画面へ戻る
    window.location.href = "login.html";
});
