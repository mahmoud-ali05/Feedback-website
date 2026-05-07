namespace Feedback.Models
{
    public class ReviewDetailViewModel
    {
        public Review Review { get; set; } = null!;
        public CommentViewModel NewComment { get; set; } = new();
    }
}
