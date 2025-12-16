namespace TestManagementAPI.Services;

public class ApiKeyAuthenticationService
{
    private static readonly Dictionary<string, ApiKeyInfo> _apiKeys = new()
    {
        { "qa-key", new ApiKeyInfo { Key = "qa-key", IsReadOnly = false } },
        { "read-key", new ApiKeyInfo { Key = "read-key", IsReadOnly = true } }
    };

    public ApiKeyInfo? ValidateApiKey(string? apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            return null;

        return _apiKeys.TryGetValue(apiKey, out var keyInfo) ? keyInfo : null;
    }

    public class ApiKeyInfo
    {
        public string Key { get; set; } = string.Empty;
        public bool IsReadOnly { get; set; }
    }
}
