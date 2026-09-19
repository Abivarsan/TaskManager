using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TaskManager.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ResetPasswordConfirmationModel : PageModel
{
    public void OnGet()
    {
    }
}
