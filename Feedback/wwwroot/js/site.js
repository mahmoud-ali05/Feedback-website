document.addEventListener("click", function (e) {
  let btn = e.target.closest(".like-btn");
  if (!btn) return;

  e.preventDefault();
  e.stopPropagation();

  // Ignore if a request is already in flight
  if (btn.classList.contains("loading")) return;
  btn.classList.add("loading");

  let id = btn.getAttribute("data-id");

  fetch(`/Reviews/Like/${id}`, { method: "POST" })
    .then((res) => {
      if (!res.ok) {
        if (res.status === 401) {
          alert("Please log in to like reviews!");
          window.location.href = "/Account/Login";
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

// Auto-focus the comment textarea when navigating to #comments
if (window.location.hash === "#comments") {
  const textarea = document.querySelector(".comment-form textarea");
  if (textarea) {
    setTimeout(() => {
      textarea.scrollIntoView({ behavior: "smooth", block: "center" });
      textarea.focus();
    }, 100); // slight delay to let the page settle
  }
}
