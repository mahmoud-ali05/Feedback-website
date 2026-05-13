using Feedback.Models;
using UsersApp.Models;

namespace Feedback.ViewModel
{
    public class ProfileViewModel
    {
        public Users User { get; set; } = null!;
        public List<Review> MyReviews { get; set; } = new();
        public List<Review> LikedReviews { get; set; } = new();
        public List<Comment> MyComments { get; set; } = new();
    }
}