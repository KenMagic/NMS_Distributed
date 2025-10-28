namespace FUNewsManagement_FE.Middleware
{
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiLoggingMiddleware> _logger;

        public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            var startTime = DateTime.UtcNow;

            // Process request
            await _next(context);

            var statusCode = context.Response.StatusCode;
            var endpoint = $"{request.Method} {request.Path}{request.QueryString}";
            var duration = DateTime.UtcNow - startTime;

            _logger.LogInformation("API Request: {Endpoint} | Status: {StatusCode} | Time: {Time} | Duration: {Duration}ms",
                endpoint, statusCode, startTime, duration.TotalMilliseconds);
        }
    }

}
