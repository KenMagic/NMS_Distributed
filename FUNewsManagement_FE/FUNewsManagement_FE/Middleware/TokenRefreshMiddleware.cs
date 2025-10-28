using FUNewsManagement_FE.Services;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public class TokenRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpClientFactory _httpFactory;

    public TokenRefreshMiddleware(RequestDelegate next, IHttpClientFactory httpFactory)
    {
        _next = next;
        _httpFactory = httpFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Cookies["jwt_token"];
        if (!string.IsNullOrEmpty(token))
        {
            var exp = GetTokenExpiration(token);
            if (exp.HasValue && exp.Value < DateTime.UtcNow.AddMinutes(5)) // refresh nếu còn <5 phút
            {
                var newToken = await RefreshJwtAsync(token);
                if (!string.IsNullOrEmpty(newToken))
                {
                    context.Response.Cookies.Append("jwt_token", newToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true
                    });
                }
            }
        }

        await _next(context);
    }

    private DateTime? GetTokenExpiration(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (expClaim == null) return null;

        var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;
        return exp;
    }

    private async Task<string?> RefreshJwtAsync(string currentToken)
    {
        var client = _httpFactory.CreateClient("CoreApi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", currentToken);

        var res = await client.PostAsync("/api/Auth/RefreshToken", null);
        if (!res.IsSuccessStatusCode) return null;

        var json = await res.Content.ReadAsStringAsync();
        // nếu JSON: { "token": "..." }
        var obj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        return obj?["token"];
    }
}

