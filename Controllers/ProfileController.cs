// Controllers/ProfileController.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        public class ProfileViewModel
        {
            [Display(Name = "Email Address")]
            public string Email { get; set; } = string.Empty;

            [StringLength(50, ErrorMessage = "Display name cannot exceed 50 characters")]
            [Display(Name = "Display Name")]
            public string? DisplayName { get; set; }

            [Display(Name = "Avatar Theme")]
            public string AvatarColor { get; set; } = "primary";

            public int TotalTasks { get; set; }
            public int CompletedTasks { get; set; }
        }

        public class ChangePasswordViewModel
        {
            [Required(ErrorMessage = "Current password is required")]
            [DataType(DataType.Password)]
            [Display(Name = "Current Password")]
            public string CurrentPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "New password is required")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New Password")]
            public string NewPassword { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm New Password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var totalTasks = await _context.UserTasks.CountAsync(t => t.UserId == user.Id);
            var completedTasks = await _context.UserTasks.CountAsync(t => t.UserId == user.Id && t.Status == Models.TaskStatus.Completed);

            var model = new ProfileViewModel
            {
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName ?? string.Empty,
                AvatarColor = user.AvatarColor ?? "primary",
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks
            };

            ViewBag.ChangePasswordModel = new ChangePasswordViewModel();
            return View(model);
        }

        // POST: /Profile/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind("DisplayName,AvatarColor")] ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            user.DisplayName = model.DisplayName?.Trim();
            user.AvatarColor = string.IsNullOrWhiteSpace(model.AvatarColor) ? "primary" : model.AvatarColor;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Your profile has been updated successfully!";
                await _signInManager.RefreshSignInAsync(user);
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update profile. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill out all password fields properly.";
                return RedirectToAction(nameof(Index));
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                var errors = string.Join("; ", changePasswordResult.Errors.Select(e => e.Description));
                TempData["ErrorMessage"] = errors;
                return RedirectToAction(nameof(Index));
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Your password has been changed successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
