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
        public IActionResult Index(string searchString, string categoryFilter)
        {
            var reviews = _context.Reviews
                                  .Include(r => r.User)
                                  .AsQueryable();

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

            return View("Feed", reviews.ToList());
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

            var vm = new ReviewDetailViewModel
            {
                Review = review,
                NewComment = new CommentViewModel { ReviewId = id }
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
        public async Task<IActionResult> Like(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var (newLikes, success) = await _reviewService.LikeReview(id, user.Id);
            if (newLikes == -1) return NotFound();
            if (!success) return BadRequest();

            return Json(new { likes = newLikes });
        }
    }
}
