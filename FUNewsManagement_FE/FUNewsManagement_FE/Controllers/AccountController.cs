using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace FUNewsManagement_FE.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBase = "/api/Account";

        public AccountController(IHttpClientFactory httpClientFactory)
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

        public async Task<IActionResult> Index(string? searchName, string? searchEmail, int? filterRole, int page = 1)
        {
            var client = CreateClient();

            var filters = new List<string>();
            if (!string.IsNullOrEmpty(searchName)) filters.Add($"contains(AccountName,'{searchName}')");
            if (!string.IsNullOrEmpty(searchEmail)) filters.Add($"contains(AccountEmail,'{searchEmail}')");
            if (filterRole.HasValue) filters.Add($"AccountRole eq {filterRole}");

            var filterStr = filters.Count > 0 ? "$filter=" + string.Join(" and ", filters) : "";
            int skip = (page - 1) * 5;
            var query = $"?$top=5&$skip={skip}&$orderby=AccountName";
            if (!string.IsNullOrEmpty(filterStr)) query += "&" + filterStr;

            var res = await client.GetAsync($"/api/Account{query}");
            var vm = new AccountIndexViewModel
            {
                Page = page,
                SearchName = searchName,
                SearchEmail = searchEmail,
                FilterRole = filterRole
            };

            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                vm.Accounts = JsonSerializer.Deserialize<List<AccountDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
            ViewBag.Roles = new SelectList(new Dictionary<int, string>
{
    {1, "Staff"},
    {2, "Lecturer"}
}, "Key", "Value", filterRole);

            return View(vm);
        }


        public IActionResult Create()
        {
            var roles = new List<SelectListItem>
    {
        new SelectListItem { Text = "Staff", Value = "1" },
        new SelectListItem { Text = "Lecturer", Value = "2" }
    };
            ViewBag.Roles = new SelectList(roles, "Value", "Text", 1);
            return View(new AccountCreateUpdateDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(AccountCreateUpdateDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = CreateClient();
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await client.PostAsync(_apiBase, content);
            if (res.IsSuccessStatusCode)
                {
                    TempData["ToastMessage"] = "Account created successfully!";
                    TempData["ToastType"] = "success";
                    return RedirectToAction(nameof(Index));
                }

            TempData["ToastMessage"] = "Create failed";
            TempData["ToastType"] = "error";
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = CreateClient();
            var res = await client.GetAsync($"{_apiBase}/{id}");
            if (!res.IsSuccessStatusCode) return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var account = JsonSerializer.Deserialize<AccountDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            var dto = new AccountCreateUpdateDto
            {
                AccountName = account.AccountName,
                AccountEmail = account.AccountEmail,
                AccountRole = account.AccountRole
            };

            var roles = new List<SelectListItem>
    {
        new SelectListItem { Text = "Staff", Value = "1" },
        new SelectListItem { Text = "Lecturer", Value = "2" }
    };
            ViewBag.Roles = new SelectList(roles, "Value", "Text", dto.AccountRole);
            ViewBag.AccountId = id;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, AccountCreateUpdateDto model, string currentPassword)
        {
            if (!ModelState.IsValid) return View(model);

            var client = CreateClient();
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PutAsync($"{_apiBase}/{id}?currentPassword={currentPassword}", content);
            if (res.IsSuccessStatusCode)
            {
                TempData["ToastMessage"] = "Account updated successfully!";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(Index));
            }

            TempData["ToastMessage"] = "Update failed";
            TempData["ToastType"] = "error";
            var roles = new List<SelectListItem>
    {
        new SelectListItem { Text = "Staff", Value = "1" },
        new SelectListItem { Text = "Lecturer", Value = "2" }
    };
            ViewBag.Roles = new SelectList(roles, "Value", "Text", model.AccountRole);
            ViewBag.AccountId = id;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var client = CreateClient();
            var res = await client.DeleteAsync($"{_apiBase}/{id}");
            if (res.IsSuccessStatusCode)
            {
                TempData["ToastMessage"] = "Account deleted successfully!";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "Delete failed!";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction(nameof(Index));
        }
        public class AccountDto
        {
            public int AccountId { get; set; }
            public string AccountName { get; set; }
            public string AccountEmail { get; set; }
            public int AccountRole { get; set; }
        }
        public class AccountIndexViewModel
        {
            public List<AccountDto> Accounts { get; set; } = new();
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 5;
            public int TotalCount { get; set; }

            public string SearchName { get; set; }
            public string SearchEmail { get; set; }
            public int? FilterRole { get; set; }

            public Dictionary<int, string> Roles { get; set; } = new()
    {
        {1, "Staff"},
        {2, "Lecturer"}
    };
        }


        public class AccountCreateUpdateDto
        {
            public string AccountName { get; set; }
            public string AccountEmail { get; set; }
            public int AccountRole { get; set; }
            public string AccountPassword { get; set; } // optional on update
        }

        public class APIResponse<T>
        {
            public int StatusCode { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
        }

    }

}
