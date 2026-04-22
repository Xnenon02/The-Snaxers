using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TheSnaxers.Filters;

public class ApiKeyFilter : IActionFilter
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyFilter> _logger;

    public ApiKeyFilter(IConfiguration configuration, ILogger<ApiKeyFilter> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var apiKey = context.HttpContext.Request.Headers["X-Api-Key"].FirstOrDefault();
        var validKey = _configuration["ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey) || apiKey != validKey)
        {
            _logger.LogWarning("Unauthorized API access attempt from {IP}",
                context.HttpContext.Connection.RemoteIpAddress);
            context.Result = new UnauthorizedObjectResult(new { message = "Invalid or missing API key" });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}