<<<<<<< Updated upstream
﻿        using System.ComponentModel.DataAnnotations;
=======
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

>>>>>>> Stashed changes
namespace Feedback.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Comment text is required")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Comment must be between 2 and 500 characters")]
        public string? Text { get; set; }

        // Deprecated: Use UserId/User instead
        // [Required(ErrorMessage = "Author name is required")]
        // [StringLength(50, MinimumLength = 2, ErrorMessage = "Author name must be between 2 and 50 characters")]
        // public string? Author { get; set; }

        [Required]
        public int ReviewId { get; set; }

        public Review? Review { get; set; }

        // New: Link to Users table
        public string? UserId { get; set; }
        public Users? User { get; set; }
    }
}
