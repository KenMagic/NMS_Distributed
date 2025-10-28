using FUNewsManagement_FE.Hubs;
using FUNewsManagement_FE.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace FUNewsManagement_FE.Controllers
{
    public class NewsController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private const int PageSize = 3;
        private readonly IHubContext<NotificationHub> _hubContext;
        public NewsController(IHttpClientFactory httpFactory, IConfiguration config, IHubContext<NotificationHub> hubContext)
        {
            _httpFactory = httpFactory;
            _config = config;
            _hubContext = hubContext;
        }
        private async Task NotifyNewArticle(NewsArticleCreateRequest article)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", article.NewsTitle);
        }

        public async Task<IActionResult> Index(string? searchTitle, string? searchAuthor, short? filterCategory,
            bool? filterStatus, DateTime? filterFrom, DateTime? filterTo, int page = 1)
        {
            var token = HttpContext.Request.Cookies["jwt_token"] ?? "";
            if (string.IsNullOrEmpty(token))
            {
                // Token không tồn tại
                return Unauthorized();
            }
            var client = _httpFactory.CreateClient("CoreApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Load Categories
            var catRes = await client.GetAsync("/api/Category");
            List<CategoryDto> categories = new();
            if (catRes.IsSuccessStatusCode)
            {
                var catJson = await catRes.Content.ReadAsStringAsync();
                var catObj = JsonSerializer.Deserialize<ApiResponse<List<CategoryDto>>>(catJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                categories = catObj?.Data ?? new();
            }

            // Build OData query
            var filters = new List<string>();
            if (!string.IsNullOrEmpty(searchTitle))
                filters.Add($"contains(NewsTitle,'{searchTitle}') or contains(Headline,'{searchTitle}') or contains(NewsContent,'{searchTitle}')");
            if (!string.IsNullOrEmpty(searchAuthor))
                filters.Add($"contains(CreatedByName,'{searchAuthor}')");
            if (filterCategory.HasValue)
                filters.Add($"CategoryId eq {filterCategory}");
            if (filterStatus.HasValue)
                filters.Add($"NewsStatus eq {filterStatus.ToString().ToLower()}");
            if (filterFrom.HasValue)
                filters.Add($"CreatedDate ge {filterFrom:yyyy-MM-dd}T00:00:00Z");
            if (filterTo.HasValue)
                filters.Add($"CreatedDate le {filterTo:yyyy-MM-dd}T23:59:59Z");

            var filterStr = filters.Count > 0 ? "$filter=" + string.Join(" and ", filters) : "";
            int skip = (page - 1) * PageSize;
            var query = $"?$top={PageSize}&$skip={skip}&$orderby=CreatedDate desc";
            if (!string.IsNullOrEmpty(filterStr))
                query += "&" + filterStr;

            // Load News
            var newsRes = await client.GetAsync("/api/News" + query);
            List<NewsArticleDto> newsList = new();
            if (newsRes.IsSuccessStatusCode)
            {
                var newsJson = await newsRes.Content.ReadAsStringAsync();

                newsList = JsonSerializer.Deserialize<List<NewsArticleDto>>(newsJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            var vm = new NewsIndexViewModel
            {
                NewsList = newsList,
                Categories = categories,
                SearchTitle = searchTitle,
                SearchAuthor = searchAuthor,
                FilterCategory = filterCategory,
                FilterStatus = filterStatus,
                FilterFrom = filterFrom,
                FilterTo = filterTo,
                Page = page
            };

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Save(NewsArticleDto dto)
        {
            var token = Request.Cookies["jwt_token"] ?? "";
            if (string.IsNullOrEmpty(token)) return Unauthorized();

            var client = _httpFactory.CreateClient("CoreApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage res;
            // Chuyển sang request model
            var request = new NewsArticleUpdateRequest
            {
                NewsTitle = dto.NewsTitle,
                Headline = dto.Headline,
                NewsContent = dto.NewsContent,
                NewsSource = dto.NewsSource,
                CategoryId = dto.CategoryId,
                NewsStatus = dto.NewsStatus,
                TagIds = dto.TagIds.ToList(),
                CurrentUserId = 1
            };
            var json = JsonSerializer.Serialize(request);
            Console.WriteLine(json);
            var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

            if (string.IsNullOrEmpty(dto.NewsArticleId))
                res = await client.PostAsync("/api/News", content);
            else
                res = await client.PutAsync($"/api/News/{dto.NewsArticleId}", content);

            if (res.IsSuccessStatusCode)
            {
                TempData["ToastMessage"] = "News article saved successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ToastMessage"] = "Save failed!";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Index));
            }
        }
        public async Task<IActionResult> Create()
        {
            var vm = new NewsArticleCreateRequest(); // empty request model

            // Load categories và tags để dropdown/multi-select
            ViewBag.Categories = new SelectList(await GetCategories(), "CategoryId", "CategoryName");
            ViewBag.Tags = await GetTags(); // list<TagDto>
            ViewBag.StatusList = new SelectList(new[]
            {
        new { Value = true, Text = "Active" },
        new { Value = false, Text = "Inactive" }
    }, "Value", "Text");

            return View(vm); // trả về Create.cshtml
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewsArticleCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await GetCategories(), "CategoryId", "CategoryName", request.CategoryId);
                ViewBag.Tags = await GetTags();
                ViewBag.StatusList = new SelectList(new[]
                {
            new { Value = true, Text = "Active" },
            new { Value = false, Text = "Inactive" }
        }, "Value", "Text", request.NewsStatus);

                return View(request);
            }

            var token = Request.Cookies["jwt_token"] ?? "";
            if (string.IsNullOrEmpty(token)) return Unauthorized();

            var client = _httpFactory.CreateClient("CoreApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Gửi request lên API
            request.CurrentUserId = 1; // hoặc decode từ token
            var json = JsonSerializer.Serialize(request);
            Console.WriteLine(json);

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var res = await client.PostAsync("/api/News", content);
            if (res.IsSuccessStatusCode)
            {
                await NotifyNewArticle(request);
                TempData["ToastMessage"] = "News article created successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ToastMessage"] = "Create failed!";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Index));
            }
        }
        // DTO Request cho Create
        public class NewsArticleCreateRequest
        {
            [Required]
            public string NewsTitle { get; set; }
            [Required]
            public string Headline { get; set; }
            [Required]
            public string NewsContent { get; set; }
            [Required]
            public string NewsSource { get; set; }
            [Required]
            public short CategoryId { get; set; }
            [Required]
            public bool NewsStatus { get; set; }
            public short? CurrentUserId { get; set; }
            public List<int>? TagIds { get; set; }
        }

        public async Task<IActionResult> Edit(string id)
        {
            var token = Request.Cookies["jwt_token"] ?? "";
            if (string.IsNullOrEmpty(token)) return Unauthorized();
            var client = _httpFactory.CreateClient("CoreApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = await client.GetAsync($"/api/News/{id}");
            if (!res.IsSuccessStatusCode) return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<ApiResponse<NewsArticleDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var news = wrapper?.Data;
            if (news == null) return NotFound();

            ViewBag.Categories = new SelectList(await GetCategories(), "CategoryId", "CategoryName", news.CategoryId);
            ViewBag.Tags = await GetTags();
            ViewBag.StatusList = new SelectList(new[]
            {
    new { Value = true, Text = "Active" },
    new { Value = false, Text = "Inactive" }
}, "Value", "Text", news.NewsStatus);

            return View(news);

        }

        // helper load categories
        private async Task<List<CategoryDto>> GetCategories()
        {
            var token = Request.Cookies["jwt_token"] ?? "";
            var client = _httpFactory.CreateClient("CoreApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await client.GetAsync("/api/Category");
            if (!res.IsSuccessStatusCode) return new List<CategoryDto>();
            var json = await res.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<ApiResponse<List<CategoryDto>>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return wrapper?.Data ?? new List<CategoryDto>();
        }
        // helper load tags
        private async Task<List<TagDto>> GetTags()
        {
            var token = Request.Cookies["jwt_token"] ?? "";
            if (string.IsNullOrEmpty(token)) return new List<TagDto>();

            var client = _httpFactory.CreateClient("CoreApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await client.GetAsync("/api/Tag");
            if (!res.IsSuccessStatusCode) return new List<TagDto>();

            var json = await res.Content.ReadAsStringAsync();

            var wrapper = JsonSerializer.Deserialize<ApiResponse<List<TagDto>>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return wrapper?.Data ?? new List<TagDto>();
        }

        // DTO cho tag
        public class TagDto
        {
            public int TagId { get; set; }
            public string TagName { get; set; } = "";
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var token = Request.Cookies["jwt_token"] ?? "";
            if (string.IsNullOrEmpty(token)) return Unauthorized();

            var client = _httpFactory.CreateClient("CoreApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await client.DeleteAsync($"/api/News/{id}");
            if (res.IsSuccessStatusCode)
            {
                TempData["ToastMessage"] = "News article deleted successfully!";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "Delete failed!";
                TempData["ToastType"] = "error";
            }
            return RedirectToAction(nameof(Index));
        }

        public class ApiResponse<T>
        {
            public int StatusCode { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
        }

        public class NewsIndexViewModel
        {
            public List<NewsArticleDto> NewsList { get; set; } = new();
            public List<CategoryDto> Categories { get; set; } = new();

            // Filter/search
            public string? SearchTitle { get; set; }
            public string? SearchAuthor { get; set; }
            public short? FilterCategory { get; set; }
            public bool? FilterStatus { get; set; }
            public DateTime? FilterFrom { get; set; }
            public DateTime? FilterTo { get; set; }

            // Pagination
            public int Page { get; set; } = 1;
        }

        public class CategoryDto
        {
            public short CategoryId { get; set; }
            public string CategoryName { get; set; } = "";
        }
        public class NewsArticleDto
        {
            public string NewsArticleId { get; set; } = "";
            public string NewsTitle { get; set; } = "";
            public string Headline { get; set; } = "";
            public string NewsContent { get; set; } = "";
            public string NewsSource { get; set; } = "";
            public short CategoryId { get; set; }
            public string CategoryName { get; set; } = "";
            public bool NewsStatus { get; set; }
            public string CreatedByName { get; set; } = "";
            public DateTime? CreatedDate { get; set; }
            public int[]? TagIds { get; set; } 
        }

        public class TagSuggestionRequest
        {
            public string Content { get; set; } = "";
        }
        public class NewsArticleUpdateRequest
        {
            public string NewsTitle { get; set; }
            public string Headline { get; set; }
            public string NewsContent { get; set; }
            public string NewsSource { get; set; }
            public short CategoryId { get; set; }
            public bool NewsStatus { get; set; }
            public short? CurrentUserId { get; set; }
            public List<int>? TagIds { get; set; }
        }
        public async Task<IActionResult> Detail(string id)
        {
            var token = HttpContext.Request.Cookies["jwt_token"];

            var client = _httpFactory.CreateClient("CoreApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            NewsArticleDto news = null;
            List<NewsArticleDto> relatedArticles = new List<NewsArticleDto>();

            // Load main news
            var response = await client.GetAsync($"/api/News/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResp = JsonSerializer.Deserialize<ApiResponse<NewsArticleDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                news = apiResp?.Data;
            }

            // Load related articles
            var relatedResp = await client.GetAsync($"/api/Related/{id}");
            if (relatedResp.IsSuccessStatusCode)
            {
                var json = await relatedResp.Content.ReadAsStringAsync();
                var apiResp = JsonSerializer.Deserialize<ApiResponse<List<NewsArticleDto>>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                relatedArticles = apiResp?.Data?.Take(3).ToList() ?? new List<NewsArticleDto>();
            }

            var vm = new NewsDetailViewModel
            {
                News = news,
                RelatedArticles = relatedArticles
            };

            return View(vm);
        }
        public class NewsDetailViewModel
        {
            public NewsArticleDto News { get; set; }
            public List<NewsArticleDto> RelatedArticles { get; set; } = new();
        }

    }
}
