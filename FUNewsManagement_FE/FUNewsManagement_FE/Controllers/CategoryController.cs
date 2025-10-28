using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FUNewsManagement_FE.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBase = "/api/Category";

        public CategoryController(IHttpClientFactory httpClientFactory)
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
        private async Task<List<CategoryDto>> GetParentCategoriesAsync()
        {
            var client = CreateClient();
            var res = await client.GetAsync("/api/Category");
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<APIResponse<List<CategoryDto>>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return data?.Data ?? new List<CategoryDto>();
        }
        public async Task<IActionResult> Index(string searchName, string searchDesc)
        {
            var client = CreateClient();
            var url = _apiBase;

            if (!string.IsNullOrEmpty(searchName) || !string.IsNullOrEmpty(searchDesc))
            {
                url += $"?name={searchName}&description={searchDesc}";
            }

            var resp = await client.GetAsync(url);
            List<CategoryDto> categories = new();
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var apiResp = JsonSerializer.Deserialize<APIResponse<List<CategoryDto>>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                categories = apiResp?.Data ?? new List<CategoryDto>();
            }

            return View(categories);
        }

        public async Task<IActionResult> Create()
        {
            var parents = await GetParentCategoriesAsync();
            ViewBag.ParentCategories = new SelectList(parents, "CategoryId", "CategoryName");

            return View(new CategoryCreateUpdateDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryCreateUpdateDto model)
        {
            var client = CreateClient();
            var parents = await GetParentCategoriesAsync();
            if (!ModelState.IsValid)
            { 
                ViewBag.ParentCategories = new SelectList(parents, "CategoryId", "CategoryName");
                return View(model);
            }
            var payload = new
            {
                CategoryId = (short)model.CategoryId,
                CategoryName = model.CategoryName,
                CategoryDesciption = model.CategoryDesciption,
                ParentCategoryId = model.ParentCategoryId.HasValue ? (short?)model.ParentCategoryId : null,
                IsActive = model.IsActive
            };
            var json = JsonSerializer.Serialize(payload);
            Console.WriteLine(json);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var res = await client.PostAsync($"{_apiBase}", content);
            if (res.IsSuccessStatusCode)
            {
                TempData["ToastMessage"] = "Category created successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index");
            }

            TempData["ToastMessage"] = "Create failed!";
            TempData["ToastType"] = "error";

            ModelState.AddModelError("", "Create failed");
            ViewBag.ParentCategories = new SelectList(parents, "CategoryId", "CategoryName");
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = CreateClient();
            var res = await client.GetAsync($"{_apiBase}/{id}");
            if (!res.IsSuccessStatusCode) return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<APIResponse<CategoryDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var editData = new CategoryCreateUpdateDto
            {
                CategoryId = data.Data.CategoryId,
                CategoryName = data.Data.CategoryName,
                CategoryDesciption = data.Data.CategoryDesciption,
                ParentCategoryId = data.Data.ParentCategoryId,
                IsActive = data.Data.IsActive
            };
            var parents = await GetParentCategoriesAsync();
            ViewBag.ParentCategories = new SelectList(parents, "CategoryId", "CategoryName");
            return View(editData);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CategoryCreateUpdateDto model)
        {
            var client = CreateClient();
            var parents = await GetParentCategoriesAsync();
            if (!ModelState.IsValid)
            {
                
                ViewBag.ParentCategories = new SelectList(parents, "CategoryId", "CategoryName");
                return View(model);
            }
            var payload = new
            {
                CategoryId = (short)model.CategoryId,
                CategoryName = model.CategoryName,
                CategoryDesciption = model.CategoryDesciption,
                ParentCategoryId = model.ParentCategoryId.HasValue ? (short?)model.ParentCategoryId : null,
                IsActive = model.IsActive
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var res = await client.PutAsync($"{_apiBase}/{id}", content);
            if (res.IsSuccessStatusCode)
            {
                TempData["ToastMessage"] = "Category updated successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index");
            }

            TempData["ToastMessage"] = "Update failed!";
            TempData["ToastType"] = "error";

            ModelState.AddModelError("", "Update failed");
            ViewBag.ParentCategories = new SelectList(parents, "CategoryId", "CategoryName");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var client = CreateClient();
            var resp = await client.DeleteAsync($"{_apiBase}/{id}");

            if (resp.IsSuccessStatusCode)
            {
                TempData["ToastMessage"] = "Category deleted successfully!";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "Cannot delete category. Maybe it contains articles.";
                TempData["ToastType"] = "error";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id, bool isActive)
        {
            var client = CreateClient();
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_apiBase}/{id}/toggle?isActive={isActive}");
            var resp = await client.SendAsync(request);

            if (resp.IsSuccessStatusCode)
            {
                TempData["ToastMessage"] = isActive ? "Category activated!" : "Category deactivated!";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "Action failed!";
                TempData["ToastType"] = "error";
            }
            return RedirectToAction(nameof(Index));
        }

        // DTOs
        public class CategoryDto
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }
            public string CategoryDesciption { get; set; }
            public int? ParentCategoryId { get; set; }
            public bool IsActive { get; set; }
            public int ArticleCount { get; set; }
        }

        public class APIResponse<T>
        {
            public int StatusCode { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
        }
        // DTO chỉ dùng để gửi dữ liệu lên BE cho Create / Update
        public class CategoryCreateUpdateDto
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = null!;
            public string CategoryDesciption { get; set; } = null!;
            public int? ParentCategoryId { get; set; }
            public bool IsActive { get; set; } = true;
        }

    }
}
