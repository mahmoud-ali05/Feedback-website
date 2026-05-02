using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Feedback.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Feedback.Services
{
    public class ReviewService
    {
        private List<Review> _reviews = new();
        private readonly IWebHostEnvironment _env;

        public ReviewService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IEnumerable<Review> GetAllReviews() => _reviews;

        public async Task AddReview(Review review, IFormFile? imageFile)
        {
            // Set default values
            review.Id = _reviews.Any() ? _reviews.Max(r => r.Id) + 1 : 1;
            review.Date = DateTime.Now;

            // Handle image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "reviews");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                review.ImageUrl = "/uploads/reviews/" + uniqueFileName;
            }

            _reviews.Add(review);
        }
    }
}