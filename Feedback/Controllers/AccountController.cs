using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsersApp.Models;
using UsersApp.ViewModels;
using Feedback.Models;
using Feedback.Data;
using Microsoft.AspNetCore.Http;
using Feedback.ViewModel;

namespace UsersApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager,
                                  ApplicationDbContext context, IWebHostEnvironment env)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            _context = context;
            _env = env;
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var myReviews = await _context.Reviews
                .Where(r => r.UserId == user.Id)
                .Include(r => r.Comments)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            var likedReviews = await _context.ReviewLikes
                .Where(rl => rl.UserId == user.Id)
                .Include(rl => rl.Review)
                    .ThenInclude(r => r!.User)
                .Select(rl => rl.Review!)
                .ToListAsync();

            var myComments = await _context.Comments
                .Where(c => c.UserId == user.Id)
                .Include(c => c.Review)
                .ToListAsync();

            var vm = new ProfileViewModel
            {
                User = user,
                MyReviews = myReviews,
                LikedReviews = likedReviews,
                MyComments = myComments
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(string? bio, IFormFile? photo)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.Bio = bio;

            if (photo != null && photo.Length > 0)
            {
                var folder = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(folder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                var filePath = Path.Combine(folder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await photo.CopyToAsync(stream);
                user.ProfilePhotoUrl = "/uploads/avatars/" + fileName;
            }

            await userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteProfilePhoto()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(user.ProfilePhotoUrl))
            {
                var filePath = Path.Combine(_env.WebRootPath, user.ProfilePhotoUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                user.ProfilePhotoUrl = null;
                await userManager.UpdateAsync(user);
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();
            if (comment.UserId != user.Id) return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Profile));
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Email or password is incorrect.");
                    return View(model);
                }
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                Users users = new Users
                {
                    FullName = model.Name,
                    Email = model.Email,
                    UserName = model.Email,
                };

                var result = await userManager.CreateAsync(users, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        if (error.Code == "DuplicateUserName")
                            continue;
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(model);
                }
            }
            return View(model);
        }

        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Something is wrong!");
                    return View(model);
                }
                else
                {
                    return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });
                }
            }
            return View(model);
        }

        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }
            return View(new ChangePasswordViewModel { Email = username });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);
                if (user != null)
                {
                    if (await userManager.CheckPasswordAsync(user, model.NewPassword))
                    {
                        ModelState.AddModelError("NewPassword",
                            "New password cannot be the same as the current password.");

                        return View(model);
                    }
                    var result = await userManager.RemovePasswordAsync(user);
                    if (result.Succeeded)
                    {

                        result = await userManager.AddPasswordAsync(user, model.NewPassword);
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {

                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }

                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Email not found!");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Something went wrong. try again.");
                return View(model);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
