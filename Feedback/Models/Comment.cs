using System.ComponentModel.DataAnnotations;
using UsersApp.Models;
namespace Feedback.Models
{

    public class Comment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Comment text is required")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Comment must be between 2 and 500 characters")]
        public string? Text { get; set; }

        public string? UserId { get; set; }
        public Users? User { get; set; }

        [Required]
        public int ReviewId { get; set; }

        public Review? Review { get; set; }
    }
}