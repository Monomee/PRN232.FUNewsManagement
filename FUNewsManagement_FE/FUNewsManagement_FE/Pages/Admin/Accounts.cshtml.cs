using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNewsManagement_FE.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AccountsModel : PageModel
{
    public void OnGet()
    {
    }
}
