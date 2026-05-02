using System;

namespace Feedback.Models   // ⚠️ this MUST match your project
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? ProductName { get; set; }
        public string? Text { get; set; }
        public string? ImageUrl { get; set; }
        public string? ExternalLink { get; set; }
        public string? Author { get; set; }
        public DateTime Date { get; set; }
        public int Likes { get; set; }
    }
}