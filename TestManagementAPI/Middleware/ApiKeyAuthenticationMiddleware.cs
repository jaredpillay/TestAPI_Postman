namespace TestManagementAPI.Middleware;

using TestManagementAPI.Services;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiKeyAuthenticationService _authService;

    public ApiKeyAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
        _authService = new ApiKeyAuthenticationService();
    }

    public async Task InvokeAsync(HttpContext context, RateLimitService rateLimitService)
    {
        var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();

        var keyInfo = _authService.ValidateApiKey(apiKey);
        if (keyInfo == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.3.2",
                title = "Unauthorized",
                status = 401,
                detail = "Missing or invalid API key. Provide 'X-API-Key' header."
            });
            return;
        }

        // Check rate limiting
        if (!rateLimitService.IsAllowed(apiKey!))
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            var remaining = rateLimitService.GetRemainingRequests(apiKey);
            context.Response.Headers["Retry-After"] = "60";
            context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc6585#section-4",
                title = "Too Many Requests",
                status = 429,
                detail = "Rate limit exceeded. Maximum 10 requests per minute."
            });
            return;
        }

        // Check if read-only key tries to modify data
        if (keyInfo.IsReadOnly && IsModifyingRequest(context.Request.Method))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                title = "Forbidden",
                status = 403,
                detail = "Your API key is read-only and cannot perform write operations."
            });
            return;
        }

        // Add rate limit info to response headers
        context.Response.OnStarting(() =>
        {
            var remaining = rateLimitService.GetRemainingRequests(apiKey);
            context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
            context.Response.Headers["X-RateLimit-Limit"] = "10";
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static bool IsModifyingRequest(string method)
    {
        return method is "POST" or "PUT" or "PATCH" or "DELETE";
    }
}

