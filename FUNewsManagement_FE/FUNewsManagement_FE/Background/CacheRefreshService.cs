using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace FUNewsManagement_FE.Background
{
    public class CacheRefreshService : BackgroundService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<CacheRefreshService> _logger;
        private readonly IConfiguration _config;

        public CacheRefreshService(IHttpClientFactory clientFactory, ILogger<CacheRefreshService> logger, IConfiguration config)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int hours = _config.GetValue<int>("BackgroundCacheIntervalHours");
            while (!stoppingToken.IsCancellationRequested)
            {
                await RefreshCache();
                await Task.Delay(TimeSpan.FromHours(hours), stoppingToken);
            }
        }

        private async Task RefreshCache()
        {
            try
            {
                var client = _clientFactory.CreateClient("CoreApi");
                var data = await client.GetFromJsonAsync<object>("articles");
                var cacheDir = Path.Combine("wwwroot", "cache");
                if (!Directory.Exists(cacheDir))
                {
                    Directory.CreateDirectory(cacheDir);
                }

                var filePath = Path.Combine(cacheDir, "articles.json");
                await File.WriteAllTextAsync(filePath, System.Text.Json.JsonSerializer.Serialize(data));
                _logger.LogInformation("Cache refreshed at {time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache refresh failed");
            }
        }
    }
}
