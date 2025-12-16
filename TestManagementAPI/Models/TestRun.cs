namespace TestManagementAPI.Models;

public class TestRun
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TestRunStatus Status { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public List<Guid> TestCaseIds { get; set; } = new();
}

public class CreateTestRunRequest
{
    public string Name { get; set; } = string.Empty;
    public TestRunStatus Status { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public List<Guid> TestCaseIds { get; set; } = new();
}

public class UpdateTestRunRequest
{
    public string Name { get; set; } = string.Empty;
    public TestRunStatus Status { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public List<Guid> TestCaseIds { get; set; } = new();
}

public class PatchTestRunRequest
{
    public string? Name { get; set; }
    public TestRunStatus? Status { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public List<Guid>? TestCaseIds { get; set; }
}
