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

    public async Task InvokeAsync(HttpContext context)
    {
        // Exclude public endpoints from authentication
        var path = context.Request.Path.Value ?? "";
        if (IsPublicEndpoint(path))
        {
            await _next(context);
            return;
        }

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

        // Store apiKey in context for use in endpoints
        context.Items["ApiKey"] = apiKey;
        context.Items["KeyInfo"] = keyInfo;

        await _next(context);
    }

    private static bool IsModifyingRequest(string method)
    {
        return method is "POST" or "PUT" or "PATCH" or "DELETE";
    }

    private static bool IsPublicEndpoint(string path)
    {
        // These endpoints don't require authentication
        return path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/openapi/v1.json", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/", StringComparison.OrdinalIgnoreCase);
    }
}

