// ログイン時に保存したJWTを取得
const token = sessionStorage.getItem("token");

const message = document.getElementById("message");
const resultArea = document.getElementById("resultArea");

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

// 管理者権限があるか確認
function hasAdminRole() {
    if (!token) {
        // JWTが保存されていない場合、ログイン画面へ戻る
        redirectToLogin();
        return false;
    }

    try {
        const payload = decodeJwtPayload(token);

        if (payload.role !== "admin") {
            // 管理者以外の場合はアカウント表示画面へ戻る
            window.location.href = "account.html";
            return false;
        }

        return true;
    } catch {
        // JWTの形式が不正な場合はログイン画面へ戻る
        redirectToLogin();
        return false;
    }
}

// ユーザ一覧を取得して表示
async function loadUsers() {
    message.textContent = "";
    resultArea.innerHTML = "";

    try {
        // JWTをAuthorizationヘッダに付与してユーザ一覧APIを呼び出し
        const response = await fetch("/api/admin/users", {
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
            message.textContent = "ユーザ管理画面を表示する権限がありません。";
            return;
        }

        if (!response.ok) {
            // API呼び出しに失敗した場合
            message.textContent = "ユーザ一覧の取得に失敗しました。";
            return;
        }

        const users = await response.json();

        if (users.length === 0) {
            message.textContent = "ユーザが登録されていません。";
            return;
        }

        renderUserTable(users);
    } catch {
        // 通信断などでfetch自体に失敗した場合
        message.textContent = "通信エラーが発生しました。";
    }
}

// ユーザ一覧テーブルを表示
function renderUserTable(users) {
    const table = document.createElement("table");
    table.className = "result-table";

    table.innerHTML = `
        <tr>
            <th>ID</th>
            <th>ユーザ名</th>
            <th>ロール</th>
            <th>操作</th>
        </tr>
    `;

    users.forEach(function (user) {
        const row = document.createElement("tr");

        const idCell = document.createElement("td");
        idCell.textContent = user.id;
        row.appendChild(idCell);

        const usernameCell = document.createElement("td");
        usernameCell.textContent = user.username;
        row.appendChild(usernameCell);

        const roleCell = document.createElement("td");
        roleCell.textContent = user.role;
        row.appendChild(roleCell);

        const actionCell = document.createElement("td");

        const editButton = document.createElement("button");
        editButton.textContent = "編集";
        editButton.className = "table-button";
        editButton.addEventListener("click", function () {
            window.location.href = `user-edit.html?id=${encodeURIComponent(user.id)}`;
        });

        const deleteButton = document.createElement("button");
        deleteButton.textContent = "削除";
        deleteButton.className = "table-button";
        deleteButton.addEventListener("click", async function () {
            await deleteUser(user.id, user.username);
        });

        actionCell.appendChild(editButton);
        actionCell.appendChild(deleteButton);
        row.appendChild(actionCell);

        table.appendChild(row);
    });

    resultArea.appendChild(table);
}

// ユーザを削除
async function deleteUser(id, username) {
    if (!confirm(`${username} を削除しますか？`)) {
        return;
    }

    message.textContent = "";

    try {
        // JWTをAuthorizationヘッダに付与してユーザ削除APIを呼び出し
        const response = await fetch(`/api/admin/users/${id}`, {
            method: "DELETE",
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
            message.textContent = "ユーザを削除する権限がありません。";
            return;
        }

        const result = await response.json();

        if (!response.ok) {
            message.textContent = result.message ?? "ユーザ削除に失敗しました。";
            return;
        }

        message.textContent = result.message;

        await loadUsers();
    } catch {
        // 通信断などでfetch自体に失敗した場合
        message.textContent = "通信エラーが発生しました。";
    }
}

document.getElementById("addButton").addEventListener("click", function () {
    // ユーザ追加画面へ遷移
    window.location.href = "user-edit.html";
});

document.getElementById("backButton").addEventListener("click", function () {
    // アカウント表示画面へ戻る
    window.location.href = "account.html";
});

// 管理者の場合のみ初期表示時にユーザ一覧を取得
if (hasAdminRole()) {
    loadUsers();
}