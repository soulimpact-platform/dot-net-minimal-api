document.getElementById("loginButton").addEventListener("click", async function () {
    const username = document.getElementById("username").value;
    const password = document.getElementById("password").value;
    const message = document.getElementById("message");

    message.textContent = "";

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

        sessionStorage.setItem("username", data.username);

        window.location.href = "account.html";
    } else {
        message.textContent = "ユーザー名またはパスワードが違います。";
    }
});