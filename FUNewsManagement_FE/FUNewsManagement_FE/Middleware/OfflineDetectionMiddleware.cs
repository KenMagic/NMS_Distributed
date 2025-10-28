namespace FUNewsManagement_FE.Middleware
{
    public class OfflineDetectionMiddleware
    {
        private readonly RequestDelegate _next;

        public OfflineDetectionMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, IHttpClientFactory clientFactory)
        {
            try
            {
                var client = clientFactory.CreateClient("CoreApi");
                await client.GetAsync("/WeatherForecast");
            }
            catch
            {
                context.Items["IsOffline"] = true;
            }

            await _next(context);
        }
    }

}
