using System.Threading.Tasks;
using Feedback.Models;
using Feedback.Data;
using Feedback.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UsersApp.Models;

namespace Feedback.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ReviewService _reviewService;
        private readonly UserManager<Users> _userManager;
        private readonly ApplicationDbContext _context;

        public ReviewsController(ReviewService reviewService, UserManager<Users> userManager, ApplicationDbContext context)
        {
            _reviewService = reviewService;
            _userManager = userManager;
            _context = context;
        }

        // ── Feed / Index page with Search ──
        public async Task<IActionResult> Index(string searchString, string categoryFilter)
        {
            var reviews = (await _reviewService.GetAllReviewsAsync()).AsEnumerable();

            if (!string.IsNullOrEmpty(searchString))
            {
                reviews = reviews.Where(r => r.ProductName.Contains(searchString)
                                          || r.Text.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(categoryFilter))
            {
                reviews = reviews.Where(r => r.Category == categoryFilter);
            }

            ViewBag.Categories = new List<string> { "Electronics", "Fashion", "Food", "Services", "Books", "Other" };

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryFilter;

            var reviewList = reviews.ToList();

            // Compute which reviews the current user has already liked
            var currentUser = await _userManager.GetUserAsync(User);
            HashSet<int> likedIds = new();
            if (currentUser != null)
            {
                likedIds = _context.ReviewLikes
                    .Where(rl => rl.UserId == currentUser.Id)
                    .Select(rl => rl.ReviewId)
                    .ToHashSet();
            }

            var viewModels = reviewList.Select(r => new ReviewCardViewModel
            {
                Review = r,
                IsLikedByCurrentUser = likedIds.Contains(r.Id)
            }).ToList();

            return View("Feed", viewModels);
        }


        // ── Details page ──
        public async Task<IActionResult> Details(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            bool isLiked = false;
            if (currentUser != null)
            {
                isLiked = await _context.ReviewLikes
                    .AnyAsync(rl => rl.ReviewId == id && rl.UserId == currentUser.Id);
            }

            var vm = new ReviewDetailViewModel
            {
                Review = review,
                NewComment = new CommentViewModel { ReviewId = id },
                IsLikedByCurrentUser = isLiked
            };

            return View(vm);
        }

        // ── Add comment ──
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(CommentViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Reload the review detail page with validation errors
                var review = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Comments)
                        .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(r => r.Id == vm.ReviewId);

                if (review == null) return NotFound();

                var detailVm = new ReviewDetailViewModel
                {
                    Review = review,
                    NewComment = vm
                };

                return View("Details", detailVm);
            }

            var user = await _userManager.GetUserAsync(User);

            var comment = new Comment
            {
                Text = vm.Text,
                ReviewId = vm.ReviewId,
                UserId = user?.Id
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = vm.ReviewId });
        }

        public IActionResult Create()
        {
            ViewBag.Categories = new List<string> { "Electronics", "Fashion", "Food", "Services", "Books", "Other" };
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Review review, IFormFile? Image)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    review.UserId = user.Id;
                }
                await _reviewService.AddReview(review, Image);
                return RedirectToAction("Index");
            }

            ViewBag.Categories = new List<string> { "Electronics", "Fashion", "Food", "Services", "Books", "Other" };
            return View(review);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Like(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var (newLikes, liked, success) = await _reviewService.LikeReview(id, user.Id);
            if (newLikes == -1) return NotFound();
            if (!success) return BadRequest();

            return Json(new { likes = newLikes, liked = liked });
        }

        // ── Edit ──
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();
            if (review.UserId != user?.Id) return Forbid();

            ViewBag.Categories = new List<string> { "Electronics", "Fashion", "Food", "Services", "Books", "Other" };
            return View(review);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(int id, Review review, IFormFile? Image)
        {
            var user = await _userManager.GetUserAsync(User);
            var existing = await _context.Reviews.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (existing == null) return NotFound();
            if (existing.UserId != user?.Id) return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new List<string> { "Electronics", "Fashion", "Food", "Services", "Books", "Other" };
                return View(review);
            }

            // Preserve fields the form doesn't submit
            review.Id = id;
            review.UserId = existing.UserId;
            review.Date = existing.Date;
            review.Likes = existing.Likes;
            review.ImageUrl = existing.ImageUrl; // UpdateReview will overwrite if new image uploaded

            await _reviewService.UpdateReview(review, Image);
            return RedirectToAction("Profile", "Account");
        }

        // ── Delete ──
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            await _reviewService.DeleteReview(id, user.Id);
            return RedirectToAction("Profile", "Account");
        }
    }
}
