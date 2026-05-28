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
    // JWTが保存されていない場合、ログイン画面へ戻る
    window.location.href = "login.html";
}

// JWTを削除してログイン画面へ戻る
function redirectToLogin() {
    sessionStorage.removeItem("token");
    window.location.href = "login.html";
}

// APIエラー時のメッセージを取得
async function getErrorMessage(response, defaultMessage) {
    try {
        const result = await response.json();
        return result.message ?? defaultMessage;
    } catch {
        return defaultMessage;
    }
}

// 編集時にユーザ情報を取得
async function loadUser() {
    if (!id) {
        // IDがない場合は新規追加画面として表示
        title.textContent = "ユーザ追加";
        passwordInput.placeholder = "新規追加時は必須です";
        return;
    }

    title.textContent = "ユーザ編集";

    try {
        // JWTをAuthorizationヘッダに付与してユーザ詳細APIを呼び出し
        const response = await fetch(`/api/admin/users/${id}`, {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (response.status === 401) {
            // JWTが無効・期限切れの場合はログイン画面へ戻る
            redirectToLogin();
            return;
        }

        if (response.status === 403) {
            // 権限不足の場合はメッセージを表示
            message.textContent = "ユーザ情報を取得する権限がありません。";
            return;
        }

        if (!response.ok) {
            // API呼び出しに失敗した場合
            message.textContent = "ユーザ情報の取得に失敗しました。";
            return;
        }

        const user = await response.json();

        usernameInput.value = user.username;
        roleSelect.value = user.role;

        // 編集時はパスワード未入力なら変更なし
        passwordInput.placeholder = "変更する場合のみ入力してください";
    } catch {
        // 通信断などでfetch自体に失敗した場合
        message.textContent = "通信エラーが発生しました。";
    }
}

// 入力内容を取得
function getRequestBody() {
    return {
        username: usernameInput.value,
        password: passwordInput.value,
        role: roleSelect.value
    };
}

// ユーザを保存
async function saveUser() {
    message.textContent = "";

    const requestBody = getRequestBody();

    const url = id
        ? `/api/admin/users/${id}`
        : "/api/admin/users";

    const method = id ? "PUT" : "POST";

    try {
        // JWTをAuthorizationヘッダに付与してユーザ保存APIを呼び出し
        const response = await fetch(url, {
            method: method,
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(requestBody)
        });

        if (response.status === 401) {
            // JWTが無効・期限切れの場合はログイン画面へ戻る
            redirectToLogin();
            return;
        }

        if (response.status === 403) {
            // 権限不足の場合はメッセージを表示
            message.textContent = "ユーザを保存する権限がありません。";
            return;
        }

        if (!response.ok) {
            // APIエラー時は、取得できる範囲でメッセージを表示
            message.textContent = await getErrorMessage(response, "ユーザ保存に失敗しました。");
            return;
        }

        const result = await response.json();

        message.textContent = result.message;

        window.location.href = "user-list.html";
    } catch {
        // 通信断などでfetch自体に失敗した場合
        message.textContent = "通信エラーが発生しました。";
    }
}

document.getElementById("saveButton").addEventListener("click", async function () {
    await saveUser();
});

document.getElementById("backButton").addEventListener("click", function () {
    // ユーザ一覧画面へ戻る
    window.location.href = "user-list.html";
});

// 初期表示時にユーザ情報を取得
loadUser();