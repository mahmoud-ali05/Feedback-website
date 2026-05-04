using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Feedback.Data;
using Feedback.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Feedback.Services
{
    public class ReviewService
    {
        private readonly IWebHostEnvironment _env;

        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IEnumerable<Review>> GetAllReviewsAsync()
        {
            return await _context.Reviews
                            .Include(r => r.Comments).ThenInclude(c => c.User)
                            .Include(r => r.User)
                            .ToListAsync();
        }

        public async Task AddReview(Review review, IFormFile? imageFile)
        {
            // Set default values
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

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
        }

        public async Task<(int likes, bool success)> LikeReview(int id, string? userId)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return (-1, false);

            // If user is not logged in, don't allow
            if (string.IsNullOrEmpty(userId)) return (review.Likes, false);

            // Check if user already liked this review
            var existingLike = await _context.Set<ReviewLike>()
                .FirstOrDefaultAsync(rl => rl.ReviewId == id && rl.UserId == userId);

            if (existingLike != null)
            {
                // User already liked → remove the like (toggle)
                _context.Set<ReviewLike>().Remove(existingLike);
                review.Likes--;
                await _context.SaveChangesAsync();
                return (review.Likes, true);
            }

            // User hasn't liked yet → add like
            _context.Set<ReviewLike>().Add(new ReviewLike
            {
                ReviewId = id,
                UserId = userId
            });
            review.Likes++;
            await _context.SaveChangesAsync();
            return (review.Likes, true);
        }
    }
}