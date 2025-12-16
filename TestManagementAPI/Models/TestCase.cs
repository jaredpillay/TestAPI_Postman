using System.ComponentModel.DataAnnotations;

namespace TestManagementAPI.Models;

public class TestCase
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TestCasePriority Priority { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> Tags { get; set; } = new();
}

public class CreateTestCaseRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 120 characters.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Priority is required.")]
    public TestCasePriority Priority { get; set; }

    public bool IsActive { get; set; } = true;

    public List<string> Tags { get; set; } = new();
}

public class UpdateTestCaseRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TestCasePriority Priority { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> Tags { get; set; } = new();
}

public class PatchTestCaseRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TestCasePriority? Priority { get; set; }
    public bool? IsActive { get; set; }
    public List<string>? Tags { get; set; }
}
