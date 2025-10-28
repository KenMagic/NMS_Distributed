using Microsoft.AspNetCore.Mvc.Filters;

namespace FUNewsManagement_FE.Middleware
{
    public class ApiLoggingFilter : IActionFilter
    {
        private readonly ILogger<ApiLoggingFilter> _logger;
        public ApiLoggingFilter(ILogger<ApiLoggingFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // trước khi gọi action
            var request = context.HttpContext.Request;
            context.HttpContext.Items["StartTime"] = DateTime.UtcNow;
            _logger.LogInformation("Executing {Method} {Path}", request.Method, request.Path);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // sau khi gọi action
            var request = context.HttpContext.Request;
            var statusCode = context.HttpContext.Response.StatusCode;
            var startTime = (DateTime)context.HttpContext.Items["StartTime"];
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("Executed {Method} {Path} | Status: {StatusCode} | Duration: {Duration}ms",
                request.Method, request.Path, statusCode, duration.TotalMilliseconds);
        }
    }

}
