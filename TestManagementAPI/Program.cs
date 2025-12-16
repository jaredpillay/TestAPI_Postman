using TestManagementAPI.Middleware;
using TestManagementAPI.Models;
using TestManagementAPI.Repositories;
using TestManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ITestCaseRepository, TestCaseRepository>();
builder.Services.AddSingleton<ITestRunRepository, TestRunRepository>();
builder.Services.AddSingleton<IDefectRepository, DefectRepository>();
builder.Services.AddSingleton<RateLimitService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

// GET all test cases with filtering and pagination
app.MapGet("/api/testcases", async (HttpContext context, ITestCaseRepository repo) =>
{
    var priority = context.Request.Query["priority"].FirstOrDefault() != null 
        ? Enum.Parse<TestCasePriority>(context.Request.Query["priority"].FirstOrDefault() ?? "")
        : null as TestCasePriority?;
    var isActive = context.Request.Query["isActive"].FirstOrDefault() != null
        ? bool.Parse(context.Request.Query["isActive"].FirstOrDefault() ?? "false")
        : null as bool?;
    var tag = context.Request.Query["tag"].FirstOrDefault();
    var page = int.TryParse(context.Request.Query["page"].FirstOrDefault(), out var p) ? p : 1;
    var pageSize = int.TryParse(context.Request.Query["pageSize"].FirstOrDefault(), out var ps) ? ps : 10;

    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;

    var (items, totalCount) = await repo.GetAllAsync(priority, isActive, tag, page, pageSize);
    return Results.Ok(new PagedResponse<TestCase>
    {
        Data = items,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    });
})
.WithName("GetTestCases");

// GET test case by id
app.MapGet("/api/testcases/{id:guid}", async (Guid id, ITestCaseRepository repo) =>
{
    var testCase = await repo.GetByIdAsync(id);
    if (testCase == null)
        return Results.NotFound(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.4", title = "Not Found", status = 404, detail = $"TestCase with id {id} not found."});
    return Results.Ok(testCase);
})
.WithName("GetTestCaseById");

// POST create test case
app.MapPost("/api/testcases", async (CreateTestCaseRequest request, ITestCaseRepository repo) =>
{
    var validationErrors = new List<string>();
    if (string.IsNullOrWhiteSpace(request.Title))
        validationErrors.Add("Title is required.");
    else if (request.Title.Length < 3 || request.Title.Length > 120)
        validationErrors.Add("Title must be between 3 and 120 characters.");

    if (validationErrors.Any())
        return Results.BadRequest(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.1", title = "One or more validation errors occurred.", status = 400, errors = new { title = validationErrors }});

    var testCase = new TestCase
    {
        Id = Guid.NewGuid(),
        Title = request.Title,
        Description = request.Description,
        Priority = request.Priority,
        IsActive = request.IsActive,
        Tags = request.Tags ?? new()
    };

    await repo.CreateAsync(testCase);
    return Results.Created($"/api/testcases/{testCase.Id}", testCase);
})
.WithName("CreateTestCase");

// PUT update test case
app.MapPut("/api/testcases/{id:guid}", async (Guid id, UpdateTestCaseRequest request, ITestCaseRepository repo) =>
{
    var existing = await repo.GetByIdAsync(id);
    if (existing == null)
        return Results.NotFound(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.4", title = "Not Found", status = 404, detail = $"TestCase with id {id} not found."});

    var validationErrors = new List<string>();
    if (string.IsNullOrWhiteSpace(request.Title))
        validationErrors.Add("Title is required.");
    else if (request.Title.Length < 3 || request.Title.Length > 120)
        validationErrors.Add("Title must be between 3 and 120 characters.");

    if (validationErrors.Any())
        return Results.BadRequest(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.1", title = "One or more validation errors occurred.", status = 400, errors = new { title = validationErrors }});

    var updated = new TestCase
    {
        Id = existing.Id,
        Title = request.Title,
        Description = request.Description,
        Priority = request.Priority,
        IsActive = request.IsActive,
        Tags = request.Tags ?? new()
    };

    await repo.UpdateAsync(id, updated);
    return Results.Ok(updated);
})
.WithName("UpdateTestCase");

// PATCH partial update test case
app.MapPatch("/api/testcases/{id:guid}", async (Guid id, PatchTestCaseRequest request, ITestCaseRepository repo) =>
{
    var existing = await repo.GetByIdAsync(id);
    if (existing == null)
        return Results.NotFound(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.4", title = "Not Found", status = 404, detail = $"TestCase with id {id} not found."});

    if (!string.IsNullOrEmpty(request.Title))
    {
        if (request.Title.Length < 3 || request.Title.Length > 120)
            return Results.BadRequest(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.1", title = "One or more validation errors occurred.", status = 400, errors = new { title = new[] { "Title must be between 3 and 120 characters." } }});
        existing.Title = request.Title;
    }

    if (request.Description != null)
        existing.Description = request.Description;

    if (request.Priority.HasValue)
        existing.Priority = request.Priority.Value;

    if (request.IsActive.HasValue)
        existing.IsActive = request.IsActive.Value;

    if (request.Tags != null)
        existing.Tags = request.Tags;

    await repo.UpdateAsync(id, existing);
    return Results.Ok(existing);
})
.WithName("PatchTestCase");

// DELETE test case
app.MapDelete("/api/testcases/{id:guid}", async (Guid id, ITestCaseRepository testCaseRepo, ITestRunRepository testRunRepo) =>
{
    var existing = await testCaseRepo.GetByIdAsync(id);
    if (existing == null)
        return Results.NotFound(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.4", title = "Not Found", status = 404, detail = $"TestCase with id {id} not found."});

    var allRuns = await testRunRepo.GetAllAsync();
    if (allRuns.Any(tr => tr.TestCaseIds.Contains(id)))
        return Results.Conflict(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.8", title = "Conflict", status = 409, detail = "Cannot delete TestCase because it is referenced by one or more TestRuns."});

    await testCaseRepo.DeleteAsync(id);
    return Results.NoContent();
})
.WithName("DeleteTestCase");

// GET all test runs
app.MapGet("/api/testruns", async (ITestRunRepository repo) =>
{
    var testRuns = await repo.GetAllAsync();
    return Results.Ok(testRuns);
})
.WithName("GetTestRuns");

// GET test run by id
app.MapGet("/api/testruns/{id:guid}", async (Guid id, ITestRunRepository repo) =>
{
    var testRun = await repo.GetByIdAsync(id);
    if (testRun == null)
        return Results.NotFound(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.4", title = "Not Found", status = 404, detail = $"TestRun with id {id} not found."});
    return Results.Ok(testRun);
})
.WithName("GetTestRunById");

// POST create test run
app.MapPost("/api/testruns", async (CreateTestRunRequest request, ITestRunRepository repo) =>
{
    var validationErrors = new List<string>();
    if (string.IsNullOrWhiteSpace(request.Name))
        validationErrors.Add("Name is required.");

    if (validationErrors.Any())
        return Results.BadRequest(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.1", title = "One or more validation errors occurred.", status = 400, errors = new { title = validationErrors }});

    var testRun = new TestRun
    {
        Id = Guid.NewGuid(),
        Name = request.Name,
        Status = request.Status,
        ExecutedAt = request.ExecutedAt,
        TestCaseIds = request.TestCaseIds ?? new()
    };

    await repo.CreateAsync(testRun);
    return Results.Created($"/api/testruns/{testRun.Id}", testRun);
})
.WithName("CreateTestRun");

// PUT update test run
app.MapPut("/api/testruns/{id:guid}", async (Guid id, UpdateTestRunRequest request, ITestRunRepository repo) =>
{
    var existing = await repo.GetByIdAsync(id);
    if (existing == null)
        return Results.NotFound(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.4", title = "Not Found", status = 404, detail = $"TestRun with id {id} not found."});

    var validationErrors = new List<string>();
    if (string.IsNullOrWhiteSpace(request.Name))
        validationErrors.Add("Name is required.");

    if (validationErrors.Any())
        return Results.BadRequest(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.1", title = "One or more validation errors occurred.", status = 400, errors = new { title = validationErrors }});

    var updated = new TestRun
    {
        Id = existing.Id,
        Name = request.Name,
        Status = request.Status,
        ExecutedAt = request.ExecutedAt,
        TestCaseIds = request.TestCaseIds ?? new()
    };

    await repo.UpdateAsync(id, updated);
    return Results.Ok(updated);
})
.WithName("UpdateTestRun");

// PATCH partial update test run
app.MapPatch("/api/testruns/{id:guid}", async (Guid id, PatchTestRunRequest request, ITestRunRepository repo) =>
{
    var existing = await repo.GetByIdAsync(id);
    if (existing == null)
        return Results.NotFound(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.4", title = "Not Found", status = 404, detail = $"TestRun with id {id} not found."});

    if (!string.IsNullOrEmpty(request.Name))
        existing.Name = request.Name;

    if (request.Status.HasValue)
        existing.Status = request.Status.Value;

    if (request.ExecutedAt != null)
        existing.ExecutedAt = request.ExecutedAt;

    if (request.TestCaseIds != null)
        existing.TestCaseIds = request.TestCaseIds;

    await repo.UpdateAsync(id, existing);
    return Results.Ok(existing);
})
.WithName("PatchTestRun");

// DELETE test run
app.MapDelete("/api/testruns/{id:guid}", async (Guid id, ITestRunRepository repo) =>
{
    var existing = await repo.GetByIdAsync(id);
    if (existing == null)
        return Results.NotFound(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.4", title = "Not Found", status = 404, detail = $"TestRun with id {id} not found."});

    await repo.DeleteAsync(id);
    return Results.NoContent();
})
.WithName("DeleteTestRun");

// GET all defects
app.MapGet("/api/defects", async (IDefectRepository repo) =>
{
    var defects = await repo.GetAllAsync();
    return Results.Ok(defects);
})
.WithName("GetDefects");

// GET defect by id
app.MapGet("/api/defects/{id:guid}", async (Guid id, IDefectRepository repo) =>
{
    var defect = await repo.GetByIdAsync(id);
    if (defect == null)
        return Results.NotFound(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.4", title = "Not Found", status = 404, detail = $"Defect with id {id} not found."});
    return Results.Ok(defect);
})
.WithName("GetDefectById");

// POST create defect
app.MapPost("/api/defects", async (CreateDefectRequest request, IDefectRepository repo) =>
{
    var validationErrors = new List<string>();
    if (string.IsNullOrWhiteSpace(request.Title))
        validationErrors.Add("Title is required.");

    if (validationErrors.Any())
        return Results.BadRequest(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.1", title = "One or more validation errors occurred.", status = 400, errors = new { title = validationErrors }});

    var defect = new Defect
    {
        Id = Guid.NewGuid(),
        Title = request.Title,
        Severity = request.Severity,
        Status = request.Status,
        LinkedTestCaseId = request.LinkedTestCaseId,
        CreatedAt = DateTime.UtcNow
    };

    await repo.CreateAsync(defect);
    return Results.Created($"/api/defects/{defect.Id}", defect);
})
.WithName("CreateDefect");

// PUT update defect
app.MapPut("/api/defects/{id:guid}", async (Guid id, UpdateDefectRequest request, IDefectRepository repo) =>
{
    var existing = await repo.GetByIdAsync(id);
    if (existing == null)
        return Results.NotFound(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.4", title = "Not Found", status = 404, detail = $"Defect with id {id} not found."});

    var validationErrors = new List<string>();
    if (string.IsNullOrWhiteSpace(request.Title))
        validationErrors.Add("Title is required.");

    if (validationErrors.Any())
        return Results.BadRequest(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.1", title = "One or more validation errors occurred.", status = 400, errors = new { title = validationErrors }});

    var updated = new Defect
    {
        Id = existing.Id,
        Title = request.Title,
        Severity = request.Severity,
        Status = request.Status,
        LinkedTestCaseId = request.LinkedTestCaseId,
        CreatedAt = existing.CreatedAt
    };

    await repo.UpdateAsync(id, updated);
    return Results.Ok(updated);
})
.WithName("UpdateDefect");

// PATCH partial update defect
app.MapPatch("/api/defects/{id:guid}", async (Guid id, PatchDefectRequest request, IDefectRepository repo) =>
{
    var existing = await repo.GetByIdAsync(id);
    if (existing == null)
        return Results.NotFound(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.4", title = "Not Found", status = 404, detail = $"Defect with id {id} not found."});

    if (!string.IsNullOrEmpty(request.Title))
        existing.Title = request.Title;

    if (request.Severity.HasValue)
        existing.Severity = request.Severity.Value;

    if (request.Status.HasValue)
        existing.Status = request.Status.Value;

    if (request.LinkedTestCaseId != null)
        existing.LinkedTestCaseId = request.LinkedTestCaseId;

    await repo.UpdateAsync(id, existing);
    return Results.Ok(existing);
})
.WithName("PatchDefect");

// DELETE defect
app.MapDelete("/api/defects/{id:guid}", async (Guid id, IDefectRepository repo) =>
{
    var existing = await repo.GetByIdAsync(id);
    if (existing == null)
        return Results.NotFound(new {type = "https://tools.ietf.org/html/rfc7231#section-6.5.4", title = "Not Found", status = 404, detail = $"Defect with id {id} not found."});

    await repo.DeleteAsync(id);
    return Results.NoContent();
})
.WithName("DeleteDefect");

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("Health");

app.Run();
