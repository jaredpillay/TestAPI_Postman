namespace TestManagementAPI.Repositories;

using TestManagementAPI.Models;

public interface IDefectRepository
{
    Task<Defect?> GetByIdAsync(Guid id);
    Task<List<Defect>> GetAllAsync();
    Task<Defect> CreateAsync(Defect defect);
    Task<Defect?> UpdateAsync(Guid id, Defect defect);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}

public class DefectRepository : IDefectRepository
{
    private static readonly List<Defect> _defects = new();
    private static readonly object _lock = new object();

    static DefectRepository()
    {
        SeedData();
    }

    private static void SeedData()
    {
        _defects.Add(new Defect
        {
            Id = new Guid("770e8400-e29b-41d4-a716-446655440001"),
            Title = "Login button not responding on mobile",
            Severity = DefectSeverity.Critical,
            Status = DefectStatus.Open,
            LinkedTestCaseId = new Guid("550e8400-e29b-41d4-a716-446655440001"),
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        });

        _defects.Add(new Defect
        {
            Id = new Guid("770e8400-e29b-41d4-a716-446655440002"),
            Title = "Email validation too strict",
            Severity = DefectSeverity.Major,
            Status = DefectStatus.InProgress,
            LinkedTestCaseId = new Guid("550e8400-e29b-41d4-a716-446655440002"),
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        });

        _defects.Add(new Defect
        {
            Id = new Guid("770e8400-e29b-41d4-a716-446655440003"),
            Title = "Typo in success message",
            Severity = DefectSeverity.Minor,
            Status = DefectStatus.Resolved,
            LinkedTestCaseId = null,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        });

        _defects.Add(new Defect
        {
            Id = new Guid("770e8400-e29b-41d4-a716-446655440004"),
            Title = "Password reset email delays",
            Severity = DefectSeverity.Major,
            Status = DefectStatus.Closed,
            LinkedTestCaseId = new Guid("550e8400-e29b-41d4-a716-446655440004"),
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        });
    }

    public Task<Defect?> GetByIdAsync(Guid id)
    {
        lock (_lock)
        {
            var defect = _defects.FirstOrDefault(d => d.Id == id);
            return Task.FromResult(defect);
        }
    }

    public Task<List<Defect>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_defects.OrderByDescending(d => d.CreatedAt).ToList());
        }
    }

    public Task<Defect> CreateAsync(Defect defect)
    {
        lock (_lock)
        {
            _defects.Add(defect);
            return Task.FromResult(defect);
        }
    }

    public Task<Defect?> UpdateAsync(Guid id, Defect defect)
    {
        lock (_lock)
        {
            var existing = _defects.FirstOrDefault(d => d.Id == id);
            if (existing == null)
                return Task.FromResult<Defect?>(null);

            existing.Title = defect.Title;
            existing.Severity = defect.Severity;
            existing.Status = defect.Status;
            existing.LinkedTestCaseId = defect.LinkedTestCaseId;

            return Task.FromResult<Defect?>(existing);
        }
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        lock (_lock)
        {
            var existing = _defects.FirstOrDefault(d => d.Id == id);
            if (existing == null)
                return Task.FromResult(false);

            _defects.Remove(existing);
            return Task.FromResult(true);
        }
    }

    public Task<bool> ExistsAsync(Guid id)
    {
        lock (_lock)
        {
            return Task.FromResult(_defects.Any(d => d.Id == id));
        }
    }
}
