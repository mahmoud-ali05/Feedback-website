using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Feedback.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }

        public ICollection<ReviewLike> ReviewLikes { get; set; } = new List<ReviewLike>();
    }
}
