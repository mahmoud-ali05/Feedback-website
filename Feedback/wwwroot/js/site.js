document.addEventListener("click", function (e) {
    if (e.target.closest(".like-btn")) {

        let btn = e.target.closest(".like-btn");
        let id = btn.getAttribute("data-id");

        fetch(`/Reviews/Like/${id}`, {
            method: "POST"
        })
            .then(res => {
                if (!res.ok) {
                    if (res.status === 401) {
                        alert("Please log in to like reviews!");
                        window.location.href = "/Account/Login";
                    }
                    return null;
                }
                return res.json();
            })
            .then(data => {
                if (data && data.likes !== undefined) {
                    btn.querySelector(".like-count").innerText = data.likes;
                    btn.classList.toggle("liked");
                }
            });
    }
});
