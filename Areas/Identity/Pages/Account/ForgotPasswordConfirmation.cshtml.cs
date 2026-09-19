using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TaskManager.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordConfirmation : PageModel
{
    public void OnGet()
    {
    }
}
