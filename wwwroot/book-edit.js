// ログイン時に保存したJWTを取得
const token = sessionStorage.getItem("token");

const params = new URLSearchParams(window.location.search);
const id = params.get("id");

const title = document.getElementById("title");
const nameInput = document.getElementById("name");
const categoryInput = document.getElementById("category");
const authorInput = document.getElementById("author");
const priceInput = document.getElementById("price");
const descriptionInput = document.getElementById("description");
const message = document.getElementById("message");

if (!token) {
    window.location.href = "login.html";
}

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

async function getErrorMessage(response, defaultMessage) {
    try {
        const result = await response.json();
        return result.message ?? defaultMessage;
    } catch {
        return defaultMessage;
    }
}

async function loadBook() {
    if (!id) {
        title.textContent = "書籍追加";
        return;
    }

    title.textContent = "書籍編集";

    try {
        const response = await fetch(`/api/admin/books/${id}`, {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (response.status === 401) {
            redirectToLogin();
            return;
        }

        if (response.status === 403) {
            message.textContent = "書籍情報を取得する権限がありません。";
            return;
        }

        if (!response.ok) {
            message.textContent = "書籍情報の取得に失敗しました。";
            return;
        }

        const book = await response.json();

        nameInput.value = book.name;
        categoryInput.value = book.category;
        authorInput.value = book.author;
        priceInput.value = book.price;
        descriptionInput.value = book.description;
    } catch {
        message.textContent = "通信エラーが発生しました。";
    }
}

function getRequestBody() {
    return {
        name: nameInput.value,
        category: categoryInput.value,
        author: authorInput.value,
        price: Number(priceInput.value),
        description: descriptionInput.value
    };
}

async function saveBook() {
    message.textContent = "";

    const requestBody = getRequestBody();
    const url = id ? `/api/admin/books/${id}` : "/api/admin/books";
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
            message.textContent = "書籍を保存する権限がありません。";
            return;
        }

        if (!response.ok) {
            message.textContent = await getErrorMessage(response, "書籍保存に失敗しました。");
            return;
        }

        const result = await response.json();

        message.textContent = result.message;

        window.location.href = "book-list.html";
    } catch {
        message.textContent = "通信エラーが発生しました。";
    }
}

document.getElementById("saveButton").addEventListener("click", async function () {
    await saveBook();
});

document.getElementById("backButton").addEventListener("click", function () {
    window.location.href = "book-list.html";
});

if (hasAdminRole()) {
    loadBook();
}
