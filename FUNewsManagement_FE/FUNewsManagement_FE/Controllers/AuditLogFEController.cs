using FUNewsManagement_FE.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

public class AuditLogFEController : Controller
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly string _apiBase = "/api/auditlog";

    public AuditLogFEController(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    private HttpClient CreateClient()
    {
        var client = _httpFactory.CreateClient("CoreApi");
        var token = Request.Cookies["jwt_token"];
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<IActionResult> Index(string? filterUser, string? filterEntity)
    {
        var client = CreateClient();

        var query = "?";
        if (!string.IsNullOrEmpty(filterUser)) query += $"userName={filterUser}&";
        if (!string.IsNullOrEmpty(filterEntity)) query += $"entityName={filterEntity}&";

        var res = await client.GetAsync(_apiBase + query.TrimEnd('&'));
        List<AuditLogDto> logs = new();
        if (res.IsSuccessStatusCode)
        {
            var json = await res.Content.ReadAsStringAsync();
            logs = JsonSerializer.Deserialize<List<AuditLogDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }

        return View(logs);
    }
}

public class AuditLogDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string Action { get; set; } = "";
    public string EntityName { get; set; } = "";
    public DateTime? Timestamp { get; set; }
    public string? BeforeData { get; set; }
    public string? AfterData { get; set; }
}
