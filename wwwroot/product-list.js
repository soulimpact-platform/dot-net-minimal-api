// ログイン時に保存したJWTを取得
const token = sessionStorage.getItem("token");

const message = document.getElementById("message");
const resultArea = document.getElementById("resultArea");

if (!token) {
    // JWTが保存されていない場合、ログイン画面へ戻る
    window.location.href = "login.html";
}

// JWTを削除してログイン画面へ戻る
function redirectToLogin() {
    sessionStorage.removeItem("token");
    window.location.href = "login.html";
}

// 書籍一覧を取得して表示
async function loadProducts() {
    message.textContent = "";
    resultArea.innerHTML = "";

    try {
        // JWTをAuthorizationヘッダに付与して書籍一覧APIを呼び出し
        const response = await fetch("/api/admin/products", {
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
            message.textContent = "書籍管理画面を表示する権限がありません。";
            return;
        }

        if (!response.ok) {
            // API呼び出しに失敗した場合
            message.textContent = "書籍一覧の取得に失敗しました。";
            return;
        }

        const products = await response.json();

        if (products.length === 0) {
            message.textContent = "書籍が登録されていません。";
            return;
        }

        renderProductTable(products);
    } catch {
        // 通信断などでfetch自体に失敗した場合
        message.textContent = "通信エラーが発生しました。";
    }
}

// 書籍一覧テーブルを表示
function renderProductTable(products) {
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

    products.forEach(function (product) {
        const row = document.createElement("tr");

        const idCell = document.createElement("td");
        idCell.textContent = product.id;
        row.appendChild(idCell);

        const nameCell = document.createElement("td");
        nameCell.textContent = product.name;
        row.appendChild(nameCell);

        const categoryCell = document.createElement("td");
        categoryCell.textContent = product.category;
        row.appendChild(categoryCell);

        const authorCell = document.createElement("td");
        authorCell.textContent = product.author;
        row.appendChild(authorCell);

        const priceCell = document.createElement("td");
        priceCell.textContent = `${product.price}円`;
        row.appendChild(priceCell);

        const actionCell = document.createElement("td");

        const editButton = document.createElement("button");
        editButton.textContent = "編集";
        editButton.className = "table-button";
        editButton.addEventListener("click", function () {
            window.location.href = `product-edit.html?id=${encodeURIComponent(product.id)}`;
        });

        const deleteButton = document.createElement("button");
        deleteButton.textContent = "削除";
        deleteButton.className = "table-button";
        deleteButton.addEventListener("click", async function () {
            await deleteProduct(product.id, product.name);
        });

        actionCell.appendChild(editButton);
        actionCell.appendChild(deleteButton);
        row.appendChild(actionCell);

        table.appendChild(row);
    });

    resultArea.appendChild(table);
}

// 書籍を削除
async function deleteProduct(id, name) {
    if (!confirm(`${name} を削除しますか？`)) {
        return;
    }

    message.textContent = "";

    try {
        // JWTをAuthorizationヘッダに付与して書籍削除APIを呼び出し
        const response = await fetch(`/api/admin/products/${id}`, {
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
            message.textContent = "書籍を削除する権限がありません。";
            return;
        }

        const result = await response.json();

        if (!response.ok) {
            message.textContent = result.message ?? "書籍削除に失敗しました。";
            return;
        }

        message.textContent = result.message;

        await loadProducts();
    } catch {
        // 通信断などでfetch自体に失敗した場合
        message.textContent = "通信エラーが発生しました。";
    }
}

document.getElementById("addButton").addEventListener("click", function () {
    // 書籍追加画面へ遷移
    window.location.href = "product-edit.html";
});

document.getElementById("backButton").addEventListener("click", function () {
    // アカウント表示画面へ戻る
    window.location.href = "account.html";
});

// 初期表示時に書籍一覧を取得
loadProducts();