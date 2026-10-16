using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FUNewsManagement_FE.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AuditLogsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuditLogsModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public void OnGet()
    {
    }
}
