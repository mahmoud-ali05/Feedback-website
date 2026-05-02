document.addEventListener("click", function (e) {
    if (e.target.closest(".like-btn")) {

        let btn = e.target.closest(".like-btn");
        let id = btn.getAttribute("data-id");

        fetch(`/Reviews/Like/${id}`, {
            method: "POST"
        })
            .then(res => res.json())
            .then(data => {
                btn.querySelector(".like-count").innerText = data.likes;
                btn.classList.toggle("liked");
            });
    }
});