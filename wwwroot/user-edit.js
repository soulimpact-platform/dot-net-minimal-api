// ログイン時に保存したJWTを取得
const token = sessionStorage.getItem("token");

// URLからユーザIDを取得
const params = new URLSearchParams(window.location.search);
const id = params.get("id");

const title = document.getElementById("title");
const usernameInput = document.getElementById("username");
const passwordInput = document.getElementById("password");
const roleSelect = document.getElementById("role");
const message = document.getElementById("message");

if (!token) {
    window.location.href = "login.html";
}

function redirectToLogin() {
    sessionStorage.removeItem("token");
    window.location.href = "login.html";
}

async function getErrorMessage(response, defaultMessage) {
    try {
        const result = await response.json();
        return result.message ?? defaultMessage;
    } catch {
        return defaultMessage;
    }
}

async function loadUser() {
    if (!id) {
        title.textContent = "ユーザ追加";
        usernameInput.disabled = false;
        passwordInput.placeholder = "新規追加時は必須です";
        return;
    }

    title.textContent = "ユーザ編集";

    try {
        const response = await fetch(`/api/admin/users/${id}`, {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (response.status === 401) {
            redirectToLogin();
            return;
        }

        if (response.status === 403) {
            message.textContent = "ユーザ情報を取得する権限がありません。";
            return;
        }

        if (!response.ok) {
            message.textContent = "ユーザ情報の取得に失敗しました。";
            return;
        }

        const user = await response.json();

        usernameInput.value = user.username;
        usernameInput.disabled = true;
        roleSelect.value = user.role;
        passwordInput.placeholder = "変更する場合のみ入力してください";
    } catch {
        message.textContent = "通信エラーが発生しました。";
    }
}

function getRequestBody() {
    return {
        username: usernameInput.value,
        password: passwordInput.value,
        role: roleSelect.value
    };
}

async function saveUser() {
    message.textContent = "";

    const requestBody = getRequestBody();
    const url = id ? `/api/admin/users/${id}` : "/api/admin/users";
    const method = id ? "PUT" : "POST";

    try {
        const response = await fetch(url, {
            method: method,
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(requestBody)
        });

        if (response.status === 401) {
            redirectToLogin();
            return;
        }

        if (response.status === 403) {
            message.textContent = "ユーザを保存する権限がありません。";
            return;
        }

        if (!response.ok) {
            message.textContent = await getErrorMessage(response, "ユーザ保存に失敗しました。");
            return;
        }

        const result = await response.json();

        message.textContent = result.message;

        window.location.href = "user-list.html";
    } catch {
        message.textContent = "通信エラーが発生しました。";
    }
}

document.getElementById("saveButton").addEventListener("click", async function () {
    await saveUser();
});

document.getElementById("backButton").addEventListener("click", function () {
    window.location.href = "user-list.html";
});

loadUser();
