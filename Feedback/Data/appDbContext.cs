using Microsoft.EntityFrameworkCore;
using Feedback.Models;

namespace Feedback.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Review> Reviews { get; set; }
        public DbSet<Comment> Comments { get; set; }

        
    }
}
