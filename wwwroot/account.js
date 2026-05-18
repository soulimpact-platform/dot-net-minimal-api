// ログイン時に保存したユーザー名を取得
const username = sessionStorage.getItem("username");

if (!username) {
    // ユーザー名が保存されていない場合、ログイン画面へ戻る
    window.location.href = "login.html";
} else {
    // ログインユーザー名を画面に表示
    document.getElementById("username").textContent = username;
}

document.getElementById("searchPageButton").addEventListener("click", function () {
    // 書籍検索画面へ遷移
    window.location.href = "search.html";
});

document.getElementById("logoutButton").addEventListener("click", function () {
    // 保存しているログイン情報を削除
    sessionStorage.removeItem("username");

    // ログイン画面へ戻る
    window.location.href = "login.html";
});