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
            return await _context.Reviews.Include(r => r.Comments).ToListAsync();
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
    }
}