namespace Feedback.Models
{
    public class ReviewCardViewModel
    {
        public Review Review { get; set; } = null!;
        public bool IsLikedByCurrentUser { get; set; }
    }
}
