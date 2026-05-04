document.addEventListener("click", function (e) {
    const btn = e.target.closest(".like-btn");
    if (!btn) return;

    const id = btn.getAttribute("data-id");

    fetch(`/Reviews/Like/${id}`, {
        method: "POST"
    })
        .then(res => {

            if (res.status === 401) {
                window.location.href = "/Account/Login";
                return null;
            }

            if (!res.ok) {
                console.error("Something went wrong");
                return null;
            }

            return res.json();
        })
        .then(data => {
            if (!data) return;

            btn.querySelector(".like-count").innerText = data.likes;
            btn.classList.toggle("liked");
        });
});