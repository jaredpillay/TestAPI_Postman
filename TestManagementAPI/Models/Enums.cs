namespace TestManagementAPI.Models;

public enum TestCasePriority
{
    Low,
    Medium,
    High
}

public enum TestRunStatus
{
    Planned,
    InProgress,
    Completed
}

public enum DefectSeverity
{
    Minor,
    Major,
    Critical
}

public enum DefectStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}
