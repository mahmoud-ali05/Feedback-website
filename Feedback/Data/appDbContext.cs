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

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("Server=db50577.databaseasp.net; Database=db50577; User Id=db50577; Password=Z#j7?6Lgd+2H; Encrypt=False; MultipleActiveResultSets=True;");
        //}
    }
}