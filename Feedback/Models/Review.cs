using System;
using System.ComponentModel.DataAnnotations;

namespace Feedback.Models   // ⚠️ this MUST match your project
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

        [Required]
        [StringLength(50)]
        public string? Author { get; set; }

        public DateTime Date { get; set; }

        public int Likes { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}