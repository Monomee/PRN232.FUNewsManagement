using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNewsManagement_FE.Pages.Staff;

[Authorize(Roles = "Staff")]
public class TagsModel : PageModel
{
    public void OnGet()
    {
    }
}
