namespace TestManagementAPI.Repositories;

using TestManagementAPI.Models;

public interface ITestRunRepository
{
    Task<TestRun?> GetByIdAsync(Guid id);
    Task<List<TestRun>> GetAllAsync();
    Task<TestRun> CreateAsync(TestRun testRun);
    Task<TestRun?> UpdateAsync(Guid id, TestRun testRun);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<List<Guid>> GetTestCaseIdsAsync(Guid id);
}

public class TestRunRepository : ITestRunRepository
{
    private static readonly List<TestRun> _testRuns = new();
    private static readonly object _lock = new object();

    static TestRunRepository()
    {
        SeedData();
    }

    private static void SeedData()
    {
        _testRuns.Add(new TestRun
        {
            Id = new Guid("660e8400-e29b-41d4-a716-446655440001"),
            Name = "Sprint 1 - Login Feature Testing",
            Status = TestRunStatus.Completed,
            ExecutedAt = DateTime.UtcNow.AddDays(-5),
            TestCaseIds = new List<Guid>
            {
                new Guid("550e8400-e29b-41d4-a716-446655440001"),
                new Guid("550e8400-e29b-41d4-a716-446655440004")
            }
        });

        _testRuns.Add(new TestRun
        {
            Id = new Guid("660e8400-e29b-41d4-a716-446655440002"),
            Name = "Sprint 2 - Registration Feature Testing",
            Status = TestRunStatus.InProgress,
            ExecutedAt = DateTime.UtcNow.AddDays(-1),
            TestCaseIds = new List<Guid>
            {
                new Guid("550e8400-e29b-41d4-a716-446655440002")
            }
        });

        _testRuns.Add(new TestRun
        {
            Id = new Guid("660e8400-e29b-41d4-a716-446655440003"),
            Name = "Performance Testing Cycle 1",
            Status = TestRunStatus.Planned,
            ExecutedAt = null,
            TestCaseIds = new List<Guid>
            {
                new Guid("550e8400-e29b-41d4-a716-446655440003")
            }
        });
    }

    public Task<TestRun?> GetByIdAsync(Guid id)
    {
        lock (_lock)
        {
            var testRun = _testRuns.FirstOrDefault(tr => tr.Id == id);
            return Task.FromResult(testRun);
        }
    }

    public Task<List<TestRun>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_testRuns.OrderBy(tr => tr.Name).ToList());
        }
    }

    public Task<TestRun> CreateAsync(TestRun testRun)
    {
        lock (_lock)
        {
            _testRuns.Add(testRun);
            return Task.FromResult(testRun);
        }
    }

    public Task<TestRun?> UpdateAsync(Guid id, TestRun testRun)
    {
        lock (_lock)
        {
            var existing = _testRuns.FirstOrDefault(tr => tr.Id == id);
            if (existing == null)
                return Task.FromResult<TestRun?>(null);

            existing.Name = testRun.Name;
            existing.Status = testRun.Status;
            existing.ExecutedAt = testRun.ExecutedAt;
            existing.TestCaseIds = testRun.TestCaseIds;

            return Task.FromResult<TestRun?>(existing);
        }
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        lock (_lock)
        {
            var existing = _testRuns.FirstOrDefault(tr => tr.Id == id);
            if (existing == null)
                return Task.FromResult(false);

            _testRuns.Remove(existing);
            return Task.FromResult(true);
        }
    }

    public Task<bool> ExistsAsync(Guid id)
    {
        lock (_lock)
        {
            return Task.FromResult(_testRuns.Any(tr => tr.Id == id));
        }
    }

    public Task<List<Guid>> GetTestCaseIdsAsync(Guid id)
    {
        lock (_lock)
        {
            var testRun = _testRuns.FirstOrDefault(tr => tr.Id == id);
            return Task.FromResult(testRun?.TestCaseIds ?? new List<Guid>());
        }
    }
}
