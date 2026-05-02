using System.Threading.Tasks;
using Feedback.Models;
using Feedback.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Feedback.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ReviewService _reviewService;

        public ReviewsController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        public IActionResult Index()
        {
            var reviews = _reviewService.GetAllReviews();
            return View("Feed", reviews);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Review review, IFormFile? Image)
        {
            if (ModelState.IsValid)
            {
                await _reviewService.AddReview(review, Image);
                return RedirectToAction(nameof(Index));
            }
            return View(review);
        }
    }
}