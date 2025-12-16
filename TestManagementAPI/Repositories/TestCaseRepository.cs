namespace TestManagementAPI.Repositories;

using TestManagementAPI.Models;

public interface ITestCaseRepository
{
    Task<TestCase?> GetByIdAsync(Guid id);
    Task<(List<TestCase> Items, int TotalCount)> GetAllAsync(TestCasePriority? priority, bool? isActive, string? tag, int page, int pageSize);
    Task<TestCase> CreateAsync(TestCase testCase);
    Task<TestCase?> UpdateAsync(Guid id, TestCase testCase);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}

public class TestCaseRepository : ITestCaseRepository
{
    private static readonly List<TestCase> _testCases = new();
    private static readonly object _lock = new object();

    static TestCaseRepository()
    {
        SeedData();
    }

    private static void SeedData()
    {
        _testCases.Add(new TestCase
        {
            Id = new Guid("550e8400-e29b-41d4-a716-446655440001"),
            Title = "Verify login with valid credentials",
            Description = "Test login functionality with correct username and password",
            Priority = TestCasePriority.High,
            IsActive = true,
            Tags = new List<string> { "auth", "login", "smoke" }
        });

        _testCases.Add(new TestCase
        {
            Id = new Guid("550e8400-e29b-41d4-a716-446655440002"),
            Title = "Verify user registration",
            Description = "Test new user registration flow",
            Priority = TestCasePriority.High,
            IsActive = true,
            Tags = new List<string> { "auth", "registration" }
        });

        _testCases.Add(new TestCase
        {
            Id = new Guid("550e8400-e29b-41d4-a716-446655440003"),
            Title = "Verify API response time",
            Description = "Test that API responds within acceptable time limits",
            Priority = TestCasePriority.Medium,
            IsActive = true,
            Tags = new List<string> { "api", "performance" }
        });

        _testCases.Add(new TestCase
        {
            Id = new Guid("550e8400-e29b-41d4-a716-446655440004"),
            Title = "Test password reset flow",
            Description = "Verify password reset functionality",
            Priority = TestCasePriority.Medium,
            IsActive = true,
            Tags = new List<string> { "auth", "reset" }
        });

        _testCases.Add(new TestCase
        {
            Id = new Guid("550e8400-e29b-41d4-a716-446655440005"),
            Title = "Verify pagination on list endpoint",
            Description = "Test pagination parameters work correctly",
            Priority = TestCasePriority.Low,
            IsActive = false,
            Tags = new List<string> { "api", "pagination" }
        });
    }

    public Task<TestCase?> GetByIdAsync(Guid id)
    {
        lock (_lock)
        {
            var testCase = _testCases.FirstOrDefault(tc => tc.Id == id);
            return Task.FromResult(testCase);
        }
    }

    public Task<(List<TestCase> Items, int TotalCount)> GetAllAsync(TestCasePriority? priority, bool? isActive, string? tag, int page, int pageSize)
    {
        lock (_lock)
        {
            var query = _testCases.AsEnumerable();

            if (priority.HasValue)
                query = query.Where(tc => tc.Priority == priority.Value);

            if (isActive.HasValue)
                query = query.Where(tc => tc.IsActive == isActive.Value);

            if (!string.IsNullOrEmpty(tag))
                query = query.Where(tc => tc.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));

            var totalCount = query.Count();
            var items = query
                .OrderBy(tc => tc.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult((items, totalCount));
        }
    }

    public Task<TestCase> CreateAsync(TestCase testCase)
    {
        lock (_lock)
        {
            _testCases.Add(testCase);
            return Task.FromResult(testCase);
        }
    }

    public Task<TestCase?> UpdateAsync(Guid id, TestCase testCase)
    {
        lock (_lock)
        {
            var existing = _testCases.FirstOrDefault(tc => tc.Id == id);
            if (existing == null)
                return Task.FromResult<TestCase?>(null);

            existing.Title = testCase.Title;
            existing.Description = testCase.Description;
            existing.Priority = testCase.Priority;
            existing.IsActive = testCase.IsActive;
            existing.Tags = testCase.Tags;

            return Task.FromResult<TestCase?>(existing);
        }
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        lock (_lock)
        {
            var existing = _testCases.FirstOrDefault(tc => tc.Id == id);
            if (existing == null)
                return Task.FromResult(false);

            _testCases.Remove(existing);
            return Task.FromResult(true);
        }
    }

    public Task<bool> ExistsAsync(Guid id)
    {
        lock (_lock)
        {
            return Task.FromResult(_testCases.Any(tc => tc.Id == id));
        }
    }
}
