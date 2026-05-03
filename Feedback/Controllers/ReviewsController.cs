using System.Threading.Tasks;
using Feedback.Models;
using Feedback.Data;
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

        public async Task<IActionResult> Index()
        {
            var reviews = await _reviewService.GetAllReviewsAsync();
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
                return RedirectToAction("Index");
            }
            return View(review);
        }
    }
}