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
                            .OrderByDescending(r => r.Date)
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



        public async Task UpdateReview(Review review, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "reviews");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await imageFile.CopyToAsync(stream);
                review.ImageUrl = "/uploads/reviews/" + uniqueFileName;
            }

            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteReview(int id, string userId)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null || review.UserId != userId) return false;

            // Delete associated image file if it exists
            if (!string.IsNullOrEmpty(review.ImageUrl))
            {
                var filePath = Path.Combine(_env.WebRootPath, review.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(int likes, bool liked, bool success)> LikeReview(int id, string? userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                var r = await _context.Reviews.FindAsync(id);
                return r == null ? (-1, false, false) : (r.Likes, false, false);
            }

            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return (-1, false, false);

            var existingLike = await _context.ReviewLikes
                .FirstOrDefaultAsync(rl => rl.ReviewId == id && rl.UserId == userId);

            if (existingLike != null)
            {
                _context.ReviewLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
                await _context.Reviews
                    .Where(r => r.Id == id && r.Likes > 0)
                    .ExecuteUpdateAsync(s => s.SetProperty(r => r.Likes, r => r.Likes - 1));
            }
            else
            {
                _context.ReviewLikes.Add(new ReviewLike { ReviewId = id, UserId = userId });
                await _context.SaveChangesAsync();
                await _context.Reviews
                    .Where(r => r.Id == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(r => r.Likes, r => r.Likes + 1));
            }

            // Read fresh from DB — bypasses EF Core cache
            var updated = await _context.Reviews
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            bool isNowLiked = existingLike == null; // if there was no existing like, we just liked it
            return (updated!.Likes, isNowLiked, true);
        }
    }
}