namespace TestManagementAPI.Services;

public class RateLimitService
{
    private static readonly Dictionary<string, List<DateTime>> _requestLog = new();
    private static readonly object _lock = new object();
    private const int MaxRequestsPerMinute = 10;

    public bool IsAllowed(string apiKey)
    {
        lock (_lock)
        {
            if (!_requestLog.ContainsKey(apiKey))
            {
                _requestLog[apiKey] = new List<DateTime>();
            }

            var now = DateTime.UtcNow;
            var oneMinuteAgo = now.AddMinutes(-1);

            // Remove old requests outside the 1-minute window
            _requestLog[apiKey].RemoveAll(t => t < oneMinuteAgo);

            if (_requestLog[apiKey].Count >= MaxRequestsPerMinute)
            {
                return false;
            }

            _requestLog[apiKey].Add(now);
            return true;
        }
    }

    public int GetRemainingRequests(string apiKey)
    {
        lock (_lock)
        {
            if (!_requestLog.ContainsKey(apiKey))
                return MaxRequestsPerMinute;

            var now = DateTime.UtcNow;
            var oneMinuteAgo = now.AddMinutes(-1);

            var recentRequests = _requestLog[apiKey].Count(t => t >= oneMinuteAgo);
            return Math.Max(0, MaxRequestsPerMinute - recentRequests);
        }
    }
}
