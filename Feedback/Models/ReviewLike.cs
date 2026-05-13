using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using UsersApp.Models;

namespace Feedback.Models
{
    public class ReviewLike
    {
        public int Id { get; set; }

        [Required]
        public int ReviewId { get; set; }

        [Required]
        public string? UserId { get; set; }

        public Review? Review { get; set; }
        public Users? User { get; set; }
    }
}