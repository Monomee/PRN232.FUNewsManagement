using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNewsManagement_FE.Pages.Lecturer;

[Authorize(Roles = "Lecturer")]
public class ArticlesModel : PageModel
{
    public void OnGet()
    {
    }
}
