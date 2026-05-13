document.addEventListener("click", function (e) {
    let btn = e.target.closest(".like-btn");
    if (!btn) return;
    e.preventDefault();
    e.stopPropagation();
    if (btn.classList.contains("loading")) return;
    btn.classList.add("loading");
    let id = btn.getAttribute("data-id");
    fetch(`/Reviews/Like/${id}`, { method: "POST" })
        .then((res) => {
            if (!res.ok) {
                if (res.status === 401) {
                    showToast("Please log in to like reviews!", "error");
                }
                return null;
            }
            return res.json();
        })
        .then((data) => {
            if (data && data.likes !== undefined) {
                btn.querySelector(".like-count").innerText = data.likes;
                btn.setAttribute("data-liked", data.liked ? "true" : "false");
                btn.classList.toggle("liked", data.liked);
                btn.querySelector(".like-heart").innerText = data.liked ? "♥" : "♡";
            }
        })
        .finally(() => {
            btn.classList.remove("loading");
        });
});

function showToast(message, type = "info") {
    const existing = document.querySelector(".upload-toast");
    if (existing) existing.remove();
    const toast = document.createElement("div");
    toast.className = "upload-toast " + type;
    toast.textContent = message;
    toast.style.left = "24px";
    toast.style.transform = "none";
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 3000);
}

if (window.location.hash === "#comments") {
    const textarea = document.querySelector(".comment-form textarea");
    if (textarea) {
        setTimeout(() => {
            textarea.scrollIntoView({ behavior: "smooth", block: "center" });
            textarea.focus();
        }, 100);
    }
}