using FUNewsManagement_FE.Background;
using FUNewsManagement_FE.Hubs;
using FUNewsManagement_FE.Middleware;
using FUNewsManagement_FE.Services;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("CoreApi", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiUrls:Core"]);
})
.AddPolicyHandler(GetRetryPolicy());

builder.Services.AddHttpClient("AnalyticsApi", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiUrls:Analytics"]);
})
.AddPolicyHandler(GetRetryPolicy());

builder.Services.AddHttpClient("AiApi", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiUrls:AI"]);
})
.AddPolicyHandler(GetRetryPolicy());
builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:5001/");
});

// HostedService
builder.Services.AddHostedService<CacheRefreshService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ApiLoggingFilter>();
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.LoginPath = "/Auth/Login";
});
builder.Services.AddDistributedMemoryCache();
var app = builder.Build();
app.MapHub<NotificationHub>("/hubs/notifications");

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseMiddleware<TokenRefreshMiddleware>();
app.UseMiddleware<ApiLoggingMiddleware>();
app.UseMiddleware<OfflineDetectionMiddleware>();
app.MapDefaultControllerRoute();
app.Run();

// ---- Polly retry policy ----
IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(Math.Pow(2, retry)));
