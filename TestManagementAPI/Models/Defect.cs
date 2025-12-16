namespace TestManagementAPI.Models;

public class Defect
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DefectSeverity Severity { get; set; }
    public DefectStatus Status { get; set; }
    public Guid? LinkedTestCaseId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDefectRequest
{
    public string Title { get; set; } = string.Empty;
    public DefectSeverity Severity { get; set; }
    public DefectStatus Status { get; set; }
    public Guid? LinkedTestCaseId { get; set; }
}

public class UpdateDefectRequest
{
    public string Title { get; set; } = string.Empty;
    public DefectSeverity Severity { get; set; }
    public DefectStatus Status { get; set; }
    public Guid? LinkedTestCaseId { get; set; }
}

public class PatchDefectRequest
{
    public string? Title { get; set; }
    public DefectSeverity? Severity { get; set; }
    public DefectStatus? Status { get; set; }
    public Guid? LinkedTestCaseId { get; set; }
}
