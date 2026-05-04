using Feedback.Models;
using Microsoft.AspNetCore.Identity;

namespace UsersApp.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }
        public ICollection<ReviewLike> ReviewLikes { get; set; } = new List<ReviewLike>();

    }
}
