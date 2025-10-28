using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement_FE.Controllers
{
    using FUNewsManagement_AnalysticsAPI.Models;
    using FUNewsManagement_FE.ViewModels;
    using Microsoft.AspNetCore.Mvc;
    using System.Net.Http.Json;

    public class DashboardController : Controller
    {
        private readonly HttpClient _httpClient;

        public DashboardController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AnalyticsApi");
        }

        public async Task<IActionResult> Index(DateTime? from, DateTime? to, short? categoryId, short? authorId, bool? status)
        {
            var query = $"?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}&categoryId={categoryId}&authorId={authorId}&status={status}";

            var dashboard = await _httpClient.GetFromJsonAsync<DashboardDto>($"/api/analytics/dashboard{query}");
            var trending = await _httpClient.GetFromJsonAsync<List<ArticleListItemDto>>($"/api/analytics/trending{query}");

            var model = new DashboardViewModel
            {
                ByCategory = dashboard?.ByCategory.ToList() ?? new List<CategoryCountDto>(),
                ByStatus = dashboard?.ByStatus.ToList() ?? new List<StatusCountDto>(),
                TrendingArticles = trending ?? new List<ArticleListItemDto>(),
                From = from,
                To = to,
                CategoryId = categoryId,
                AuthorId = authorId,
                Status = status
            };

            return View(model);
        }

        public async Task<IActionResult> ExportExcel(DateTime? from, DateTime? to, short? categoryId, short? authorId, bool? status)
        {
            var query = $"?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}&categoryId={categoryId}&authorId={authorId}&status={status}";
            var response = await _httpClient.GetAsync($"/api/analytics/export{query}");
            var content = await response.Content.ReadAsByteArrayAsync();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "dashboard.xlsx");
        }
    }

}
