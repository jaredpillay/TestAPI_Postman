namespace TestManagementAPI.Middleware;

using System.Text.Json;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestResponseLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        try
        {
            var request = context.Request;
            var requestLog = new
            {
                Timestamp = DateTime.UtcNow,
                Method = request.Method,
                Path = request.Path.Value,
                QueryString = request.QueryString.Value,
                RemoteIP = context.Connection.RemoteIpAddress?.ToString()
            };

            logger.LogInformation("Request: {@Request}", requestLog);

            await _next(context);

            var responseLog = new
            {
                Timestamp = DateTime.UtcNow,
                StatusCode = context.Response.StatusCode,
                Method = request.Method,
                Path = request.Path.Value
            };

            logger.LogInformation("Response: {@Response}", responseLog);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in RequestResponseLoggingMiddleware");
            throw;
        }
    }
}
