using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Feedback.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(100)]
        public string? ProductName { get; set; }

        [Required]
        [StringLength(1000)]
        public string? Text { get; set; }

        public string? ImageUrl { get; set; }

        [Url]
        public string? ExternalLink { get; set; }

        public string? UserId { get; set; }
        public Users? User { get; set; }

        public DateTime Date { get; set; }

        public int Likes { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<ReviewLike> ReviewLikes { get; set; } = new List<ReviewLike>();
    }
}
