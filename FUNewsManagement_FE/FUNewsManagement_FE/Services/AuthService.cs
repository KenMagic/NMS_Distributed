using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FUNewsManagement_FE.Services
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthService(IHttpClientFactory factory, IHttpContextAccessor accessor)
        {
            _httpClientFactory = factory;
            _contextAccessor = accessor;
        }

        // Login: lưu token vào cookie
        public async Task<bool> LoginAsync(string email, string password)
        {
            // Gọi API login
            var client = _httpClientFactory.CreateClient("CoreApi");
            var loginData = new { Email = email, Password = password };
            var res = await client.PostAsJsonAsync("/api/Auth/login", loginData);

            if (!res.IsSuccessStatusCode) return false;

            var wrapper = await res.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
            var content = wrapper?.Data;
            if (content == null || string.IsNullOrEmpty(content.AccessToken)) return false;
            var accessExpire = DateTime.UtcNow.AddHours(1); // token access sống 1h
            var refreshExpire = DateTime.UtcNow.AddDays(7); // refresh token sống 7d

            var cookieOptionsAccess = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = accessExpire
            };

            var cookieOptionsRefresh = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshExpire
            };

            _contextAccessor.HttpContext.Response.Cookies.Append("jwt_token", content.AccessToken, cookieOptionsAccess);
            _contextAccessor.HttpContext.Response.Cookies.Append("refresh_token", content.RefreshToken, cookieOptionsRefresh);


            return true;
        }

        public class ApiResponse<T>
        {
            public int StatusCode { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
        }

        public class LoginResponseDto
        {
            public string AccessToken { get; set; } = "";
            public string RefreshToken { get; set; } = "";
        }



        // Refresh token
        public async Task<bool> RefreshTokenAsync()
        {
            var refreshToken = _contextAccessor.HttpContext!.Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var client = _httpClientFactory.CreateClient("CoreApi");
            var response = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken });

            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (result == null) return false;
            // Nếu result.Expiration là DateTime
            var expiration = result.Expiration;

            // Kiểm tra hợp lệ, fallback nếu cần
            if (expiration <= DateTime.UtcNow || expiration > DateTime.MaxValue.AddHours(-1))
            {
                expiration = DateTime.UtcNow.AddHours(1); // default 1 giờ
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = new DateTimeOffset(expiration)
            };



            _contextAccessor.HttpContext.Response.Cookies.Append("jwt_token", result.AccessToken, cookieOptions);
            _contextAccessor.HttpContext.Response.Cookies.Append("refresh_token", result.RefreshToken, cookieOptions);

            return true;
        }

        // Hàm lấy token từ cookie để gán vào HttpClient
        public string? GetAccessToken()
        {
            return _contextAccessor.HttpContext?.Request.Cookies["jwt_token"];
        }

        // Hàm tạo HttpClient đã gán Authorization header
        public HttpClient CreateAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient("CoreApi");
            var token = GetAccessToken();
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}
