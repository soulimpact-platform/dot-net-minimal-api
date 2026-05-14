const username = sessionStorage.getItem("username");

if (!username) {
    window.location.href = "login.html";
} else {
    document.getElementById("username").textContent = username;
}

document.getElementById("logoutButton").addEventListener("click", function () {
    sessionStorage.removeItem("username");
    window.location.href = "login.html";
});