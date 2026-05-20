document.getElementById("loginButton").addEventListener("click", async function () {
    // 入力されたユーザー名とパスワードを取得
    const username = document.getElementById("username").value;
    const password = document.getElementById("password").value;
    const message = document.getElementById("message");

    // 前回のメッセージをクリア
    message.textContent = "";

    // ログインAPIへユーザー名とパスワードを送信
    const response = await fetch("/api/login", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            username: username,
            password: password
        })
    });

    if (response.ok) {
        const data = await response.json();

        // ログイン成功時に返却されたJWTを保存
        sessionStorage.setItem("token", data.token);

        // アカウント表示画面へ遷移
        window.location.href = "account.html";
    } else {
        // 認証に失敗した場合はエラーメッセージを表示
        message.textContent = "ユーザー名またはパスワードが違います。";
    }
});