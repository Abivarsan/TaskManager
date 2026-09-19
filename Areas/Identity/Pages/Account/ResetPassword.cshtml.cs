using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TaskManager.Models;

namespace TaskManager.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ResetPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ResetPasswordModel> _logger;

    public ResetPasswordModel(UserManager<ApplicationUser> userManager, ILogger<ResetPasswordModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;
    }

    public IActionResult OnGet(string? code = null, string? email = null)
    {
        if (code == null)
        {
            return BadRequest("A valid security code must be supplied for password reset.");
        }

        string decodedCode;
        try
        {
            decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch
        {
            decodedCode = code;
        }

        Input = new InputModel
        {
            Code = decodedCode,
            Email = email ?? string.Empty
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
        if (result.Succeeded)
        {
            _logger.LogInformation("Password reset succeeded for user {Email}", Input.Email);
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}
