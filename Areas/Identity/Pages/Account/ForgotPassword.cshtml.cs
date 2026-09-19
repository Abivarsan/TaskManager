using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TaskManager.Models;

namespace TaskManager.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ForgotPasswordModel> _logger;

    public ForgotPasswordModel(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        ILogger<ForgotPasswordModel> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user != null)
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code, email = Input.Email },
                    protocol: Request.Scheme);

                var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Your Password</title>
</head>
<body style='margin: 0; padding: 0; background-color: #0f172a; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; color: #f8fafc;'>
    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='background-color: #0f172a; padding: 40px 15px;'>
        <tr>
            <td align='center'>
                <table role='presentation' width='100%' style='max-width: 520px; background: #1e293b; border-radius: 16px; border: 1px solid rgba(255, 255, 255, 0.1); box-shadow: 0 20px 40px rgba(0,0,0,0.5); overflow: hidden;'>
                    <!-- Header -->
                    <tr>
                        <td style='padding: 35px 35px 25px; text-align: center; background: linear-gradient(135deg, rgba(99, 102, 241, 0.2) 0%, rgba(217, 70, 239, 0.1) 100%); border-bottom: 1px solid rgba(255, 255, 255, 0.08);'>
                            <div style='display: inline-block; width: 48px; height: 48px; line-height: 48px; border-radius: 12px; background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 50%, #d946ef 100%); color: #ffffff; font-size: 24px; font-weight: bold; margin-bottom: 12px;'>
                                &#x2714;
                            </div>
                            <h1 style='margin: 0; font-size: 24px; font-weight: 800; color: #ffffff; letter-spacing: -0.02em;'>
                                Task<span style='color: #818cf8;'>Manager</span>
                            </h1>
                        </td>
                    </tr>
                    <!-- Body Content -->
                    <tr>
                        <td style='padding: 35px; text-align: left;'>
                            <h2 style='margin: 0 0 16px; font-size: 20px; font-weight: 700; color: #ffffff;'>
                                Password Reset Request
                            </h2>
                            <p style='margin: 0 0 20px; font-size: 15px; line-height: 1.6; color: #94a3b8;'>
                                Hello,
                            </p>
                            <p style='margin: 0 0 28px; font-size: 15px; line-height: 1.6; color: #94a3b8;'>
                                We received a request to reset the password for your TaskManager account associated with <strong style='color: #f8fafc;'>{HtmlEncoder.Default.Encode(Input.Email)}</strong>.
                            </p>
                            
                            <!-- Action Button -->
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}' target='_blank' style='display: inline-block; background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 50%, #d946ef 100%); color: #ffffff; text-decoration: none; font-weight: 700; font-size: 15px; padding: 14px 32px; border-radius: 10px; box-shadow: 0 6px 20px rgba(99, 102, 241, 0.4);'>
                                    Reset Password
                                </a>
                            </div>

                            <p style='margin: 24px 0 0; font-size: 13px; line-height: 1.6; color: #64748b;'>
                                If you did not request a password reset, you can safely ignore this email. Your password will remain unchanged.
                            </p>
                            <hr style='border: none; border-top: 1px solid rgba(255, 255, 255, 0.08); margin: 25px 0;' />
                            <p style='margin: 0; font-size: 12px; line-height: 1.5; color: #64748b; word-break: break-all;'>
                                Having trouble with the button? Copy and paste this URL into your browser:<br>
                                <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}' style='color: #818cf8; text-decoration: underline;'>{HtmlEncoder.Default.Encode(callbackUrl!)}</a>
                            </p>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style='padding: 20px 35px; background: rgba(15, 23, 42, 0.5); border-top: 1px solid rgba(255, 255, 255, 0.05); text-align: center;'>
                            <p style='margin: 0; font-size: 12px; color: #475569;'>
                                &copy; {DateTime.UtcNow.Year} TaskManager. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

                try
                {
                    await _emailSender.SendEmailAsync(Input.Email, "Reset Your TaskManager Password", emailBody);
                    _logger.LogInformation("Password reset email sent to {Email}", Input.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed sending password reset email to {Email}", Input.Email);
                }
            }

            // Always redirect to confirmation to prevent account enumeration
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        return Page();
    }
}
