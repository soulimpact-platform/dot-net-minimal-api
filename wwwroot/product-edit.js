// ログイン時に保存したJWTを取得
const token = sessionStorage.getItem("token");

// URLから書籍IDを取得
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

// 編集時に書籍情報を取得
async function loadProduct() {
    if (!id) {
        // IDがない場合は新規追加画面として表示
        title.textContent = "書籍追加";
        return;
    }

    title.textContent = "書籍編集";

    try {
        // JWTをAuthorizationヘッダに付与して書籍詳細APIを呼び出し
        const response = await fetch(`/api/admin/products/${id}`, {
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
            message.textContent = "書籍情報を取得する権限がありません。";
            return;
        }

        if (!response.ok) {
            // API呼び出しに失敗した場合
            message.textContent = "書籍情報の取得に失敗しました。";
            return;
        }

        const product = await response.json();

        nameInput.value = product.name;
        categoryInput.value = product.category;
        authorInput.value = product.author;
        priceInput.value = product.price;
        descriptionInput.value = product.description;
    } catch {
        // 通信断などでfetch自体に失敗した場合
        message.textContent = "通信エラーが発生しました。";
    }
}

// 入力内容を取得
function getRequestBody() {
    return {
        name: nameInput.value,
        category: categoryInput.value,
        author: authorInput.value,
        price: Number(priceInput.value),
        description: descriptionInput.value
    };
}

// 書籍を保存
async function saveProduct() {
    message.textContent = "";

    const requestBody = getRequestBody();

    const url = id
        ? `/api/admin/products/${id}`
        : "/api/admin/products";

    const method = id ? "PUT" : "POST";

    try {
        // JWTをAuthorizationヘッダに付与して書籍保存APIを呼び出し
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
            message.textContent = "書籍を保存する権限がありません。";
            return;
        }

        if (!response.ok) {
            // APIエラー時は、取得できる範囲でメッセージを表示
            message.textContent = await getErrorMessage(response, "書籍保存に失敗しました。");
            return;
        }

        const result = await response.json();

        message.textContent = result.message;

        window.location.href = "product-list.html";
    } catch {
        // 通信断などでfetch自体に失敗した場合
        message.textContent = "通信エラーが発生しました。";
    }
}

document.getElementById("saveButton").addEventListener("click", async function () {
    await saveProduct();
});

document.getElementById("backButton").addEventListener("click", function () {
    // 書籍一覧画面へ戻る
    window.location.href = "product-list.html";
});

// 初期表示時に書籍情報を取得
loadProduct();