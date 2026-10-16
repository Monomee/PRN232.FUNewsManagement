using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNewsManagement_FE.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ReportsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ReportsModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetTestPollyAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AnalyticsApiClient");
            var response = await client.GetAsync("analytics/test-polly");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Polly đã thử hết số lần retry nhưng không thể kết nối tới Analytics API: " + ex.Message });
        }
    }
}

