using System.Threading.Tasks;
using Feedback.Models;
using Feedback.Data;
using Feedback.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Feedback.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ReviewService _reviewService;
        private readonly UserManager<Users> _userManager;

        public ReviewsController(ReviewService reviewService, UserManager<Users> userManager)
        {
            _reviewService = reviewService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var reviews = await _reviewService.GetAllReviewsAsync();
            return View("Feed", reviews);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
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
            return View(review);
        }

        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var (newLikes, success) = await _reviewService.LikeReview(id, user.Id);
            if (newLikes == -1) return NotFound();
            if (!success) return BadRequest();

            return Json(new { likes = newLikes });
        }
    }
}
