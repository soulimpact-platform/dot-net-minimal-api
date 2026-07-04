// ログイン時に保存したJWTを取得
const token = sessionStorage.getItem("token");

const message = document.getElementById("message");
const resultArea = document.getElementById("resultArea");

function redirectToLogin() {
    sessionStorage.removeItem("token");
    window.location.href = "login.html";
}

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

async function getErrorMessage(response, defaultMessage) {
    try {
        const result = await response.json();
        return result.message ?? defaultMessage;
    } catch {
        return defaultMessage;
    }
}

function hasAdminRole() {
    if (!token) {
        redirectToLogin();
        return false;
    }

    try {
        const payload = decodeJwtPayload(token);

        if (payload.role !== "admin") {
            window.location.href = "account.html";
            return false;
        }

        return true;
    } catch {
        redirectToLogin();
        return false;
    }
}

async function loadBooks() {
    message.textContent = "";
    resultArea.innerHTML = "";

    try {
        const response = await fetch("/api/admin/books", {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (response.status === 401) {
            redirectToLogin();
            return;
        }

        if (response.status === 403) {
            message.textContent = "書籍管理画面を表示する権限がありません。";
            return;
        }

        if (!response.ok) {
            message.textContent = "書籍一覧の取得に失敗しました。";
            return;
        }

        const books = await response.json();

        if (books.length === 0) {
            message.textContent = "書籍が登録されていません。";
            return;
        }

        renderBookTable(books);
    } catch {
        message.textContent = "通信エラーが発生しました。";
    }
}

function renderBookTable(books) {
    const table = document.createElement("table");
    table.className = "result-table";

    table.innerHTML = `
        <tr>
            <th>ID</th>
            <th>書籍名</th>
            <th>カテゴリ</th>
            <th>著者</th>
            <th>価格</th>
            <th>操作</th>
        </tr>
    `;

    books.forEach(function (book) {
        const row = document.createElement("tr");

        const idCell = document.createElement("td");
        idCell.textContent = book.id;
        row.appendChild(idCell);

        const nameCell = document.createElement("td");
        nameCell.textContent = book.name;
        row.appendChild(nameCell);

        const categoryCell = document.createElement("td");
        categoryCell.textContent = book.category;
        row.appendChild(categoryCell);

        const authorCell = document.createElement("td");
        authorCell.textContent = book.author;
        row.appendChild(authorCell);

        const priceCell = document.createElement("td");
        priceCell.textContent = `${book.price}円`;
        row.appendChild(priceCell);

        const actionCell = document.createElement("td");

        const editButton = document.createElement("button");
        editButton.textContent = "編集";
        editButton.className = "table-button";
        editButton.addEventListener("click", function () {
            window.location.href = `book-edit.html?id=${encodeURIComponent(book.id)}`;
        });

        const deleteButton = document.createElement("button");
        deleteButton.textContent = "削除";
        deleteButton.className = "table-button";
        deleteButton.addEventListener("click", async function () {
            await deleteBook(book.id, book.name);
        });

        actionCell.appendChild(editButton);
        actionCell.appendChild(deleteButton);
        row.appendChild(actionCell);

        table.appendChild(row);
    });

    resultArea.appendChild(table);
}

async function deleteBook(id, name) {
    if (!confirm(`${name} を削除しますか？`)) {
        return;
    }

    message.textContent = "";

    try {
        const response = await fetch(`/api/admin/books/${id}`, {
            method: "DELETE",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (response.status === 401) {
            redirectToLogin();
            return;
        }

        if (response.status === 403) {
            message.textContent = "書籍を削除する権限がありません。";
            return;
        }

        if (!response.ok) {
            message.textContent = await getErrorMessage(response, "書籍削除に失敗しました。");
            return;
        }

        const result = await response.json();

        message.textContent = result.message;

        await loadBooks();
    } catch {
        message.textContent = "通信エラーが発生しました。";
    }
}

document.getElementById("addButton").addEventListener("click", function () {
    window.location.href = "book-edit.html";
});

document.getElementById("backButton").addEventListener("click", function () {
    window.location.href = "account.html";
});

if (hasAdminRole()) {
    loadBooks();
}
