using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FUNewsManagement_FE.Controllers
{
    public class TagController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBase = "/api/Tag";

        public TagController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient("CoreApi");
            var token = Request.Cookies["jwt_token"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<IActionResult> Index(string searchTag)
        {
            var client = CreateClient();
            var url = _apiBase;
            if (!string.IsNullOrEmpty(searchTag))
                url += $"?tagName={searchTag}";

            var resp = await client.GetAsync(url);
            List<TagDto> tags = new();
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var apiResp = JsonSerializer.Deserialize<APIResponse<List<TagDto>>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                tags = apiResp?.Data ?? new List<TagDto>();
            }
            ViewBag.SearchTag = searchTag;
            return View(tags);
        }

        public IActionResult Create() => View(new TagDto());

        [HttpPost]
        public async Task<IActionResult> Create(TagDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = CreateClient();
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await client.PostAsync(_apiBase, content);
            if (res.IsSuccessStatusCode)
                {
                    TempData["ToastMessage"] = "Tag created successfully!";
                    TempData["ToastType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ToastMessage"] = "Create failed. Maybe tag name already exists.";
                    TempData["ToastType"] = "error";
                    return RedirectToAction(nameof(Index));
                }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = CreateClient();
            var res = await client.GetAsync($"{_apiBase}/{id}");
            if (!res.IsSuccessStatusCode) return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<APIResponse<TagDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(data.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TagDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = CreateClient();
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await client.PutAsync($"{_apiBase}/{model.TagId}", content);
            if (res.IsSuccessStatusCode)
            {
                TempData["ToastMessage"] = "Tag updated successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ToastMessage"] = "Update failed. Maybe tag name already exists.";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var client = CreateClient();
            var res = await client.DeleteAsync($"{_apiBase}/{id}");
            if (res.IsSuccessStatusCode)
            {
                TempData["ToastMessage"] = "Tag deleted successfully!";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "Cannot delete tag. Maybe it is used by articles.";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Articles(int id)
        {
            var client = CreateClient();

            // Lấy danh sách bài viết của tag
            var res = await client.GetAsync($"{_apiBase}/{id}/Articles");
            List<ArticleDto> articles = new();
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                var apiResp = JsonSerializer.Deserialize<APIResponse<List<ArticleDto>>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                articles = apiResp?.Data ?? new List<ArticleDto>();
            }

            // Lấy thông tin tag (tên)
            var tagRes = await client.GetAsync($"{_apiBase}/{id}");
            string tagName = "";
            if (tagRes.IsSuccessStatusCode)
            {
                var tagJson = await tagRes.Content.ReadAsStringAsync();
                var tagApiResp = JsonSerializer.Deserialize<APIResponse<TagDto>>(tagJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                tagName = tagApiResp?.Data?.TagName ?? "";
            }

            var model = new TagArticlesViewModel
            {
                TagId = id,
                TagName = tagName,
                Articles = articles
            };

            return View(model);
        }


        // DTOs
        public class TagDto
        {
            public int TagId { get; set; }
            public string TagName { get; set; }
            public string? Note { get; set; }
        }

        public class ArticleDto
        {
            public string NewsTitle { get; set; }
            public string CategoryName { get; set; }
            public string CreatedByName { get; set; }
            public DateTime? CreatedDate { get; set; }
            public bool NewsStatus { get; set; }
        }

        public class APIResponse<T>
        {
            public int StatusCode { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
        }
        public class TagArticlesViewModel
        {
            public int TagId { get; set; }
            public string TagName { get; set; } = "";
            public List<ArticleDto> Articles { get; set; } = new();
        }

    }
}
