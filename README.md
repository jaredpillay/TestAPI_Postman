# Test Management API

A minimal REST API built with .NET 10 for QA practice and interview preparation. Features comprehensive CRUD operations, API key authentication, rate limiting, and realistic error handling.

## Tech Stack

- **.NET 10.0** minimal APIs
- **In-memory repositories** (no database setup required)
- **Swagger/OpenAPI** documentation
- **ProblemDetails** for consistent error responses
- **API Key authentication** with read/write and read-only modes
- **Rate limiting** (10 requests/minute per key)
- **Request/response logging middleware**

## Quick Start

### Prerequisites

- .NET 10.0 SDK installed ([download](https://dotnet.microsoft.com/en-us/download))

### Running Locally

```bash
cd TestManagementAPI
dotnet run
```

The API will start on **`http://localhost:5106`**

Access Swagger UI: **http://localhost:5106/swagger/ui**

## API Overview

### Base URL
```
http://localhost:5106
```

### Authentication
All endpoints require the `X-API-Key` header:

```bash
curl -H "X-API-Key: qa-key" http://localhost:5106/health
```

**Available API Keys:**
- `qa-key` - Full read/write access
- `read-key` - Read-only access (GET only)

### Response Format

All responses use **ProblemDetails** (RFC 7807) format for consistency:

**Success (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "title": "Verify login with valid credentials",
  "priority": "High",
  "isActive": true,
  "tags": ["auth", "login"]
}
```

**Error (400):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "title": ["Title must be between 3 and 120 characters."]
  }
}
```

---

## Domain Models

### TestCase
```json
{
  "id": "guid",
  "title": "string (3-120 chars, required)",
  "description": "string (optional)",
  "priority": "Low|Medium|High",
  "isActive": "boolean",
  "tags": ["string"]
}
```

### TestRun
```json
{
  "id": "guid",
  "name": "string (required)",
  "status": "Planned|InProgress|Completed",
  "executedAt": "datetime (nullable)",
  "testCaseIds": ["guid"]
}
```

### Defect
```json
{
  "id": "guid",
  "title": "string (required)",
  "severity": "Minor|Major|Critical",
  "status": "Open|InProgress|Resolved|Closed",
  "linkedTestCaseId": "guid (nullable)",
  "createdAt": "datetime"
}
```

---

## Endpoints

### Test Cases

#### GET /api/testcases
Get all test cases with optional filtering and pagination.

**Query Parameters:**
- `priority` (optional): Filter by priority (Low, Medium, High)
- `isActive` (optional): Filter by active status (true/false)
- `tag` (optional): Filter by tag name
- `page` (optional, default=1): Page number
- `pageSize` (optional, default=10, max=100): Items per page

**Example:**
```bash
curl -H "X-API-Key: qa-key" \
  "http://localhost:5106/api/testcases?priority=High&isActive=true&page=1&pageSize=10"
```

**Response (200):**
```json
{
  "data": [...],
  "totalCount": 15,
  "page": 1,
  "pageSize": 10
}
```

---

#### GET /api/testcases/{id}
Get a specific test case by ID.

**Example:**
```bash
curl -H "X-API-Key: qa-key" \
  http://localhost:5106/api/testcases/550e8400-e29b-41d4-a716-446655440001
```

**Response (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "title": "Verify login with valid credentials",
  "description": "Test login functionality with correct username and password",
  "priority": "High",
  "isActive": true,
  "tags": ["auth", "login", "smoke"]
}
```

**Response (404):** Not found

---

#### POST /api/testcases
Create a new test case.

**Request:**
```bash
curl -X POST -H "X-API-Key: qa-key" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test new feature endpoint",
    "description": "Verify endpoint returns correct data",
    "priority": "High",
    "isActive": true,
    "tags": ["api", "feature"]
  }' \
  http://localhost:5106/api/testcases
```

**Response (201 Created):**
```json
{
  "id": "new-guid",
  "title": "Test new feature endpoint",
  ...
}
```

**Response (400):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "title": ["Title is required.", "Title must be between 3 and 120 characters."]
  }
}
```

---

#### PUT /api/testcases/{id}
Fully update a test case (all fields required).

**Request:**
```bash
curl -X PUT -H "X-API-Key: qa-key" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Updated title",
    "description": "Updated description",
    "priority": "Low",
    "isActive": false,
    "tags": ["updated"]
  }' \
  http://localhost:5106/api/testcases/550e8400-e29b-41d4-a716-446655440001
```

**Response (200):** Updated test case

---

#### PATCH /api/testcases/{id}
Partially update a test case (only provided fields are updated).

**Request:**
```bash
curl -X PATCH -H "X-API-Key: qa-key" \
  -H "Content-Type: application/json" \
  -d '{
    "priority": "High"
  }' \
  http://localhost:5106/api/testcases/550e8400-e29b-41d4-a716-446655440001
```

**Response (200):** Updated test case

---

#### DELETE /api/testcases/{id}
Delete a test case. **Cannot delete if referenced by a TestRun.**

**Request:**
```bash
curl -X DELETE -H "X-API-Key: qa-key" \
  http://localhost:5106/api/testcases/550e8400-e29b-41d4-a716-446655440005
```

**Response (204 No Content):** Success (empty body)

**Response (409 Conflict):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "Cannot delete TestCase because it is referenced by one or more TestRuns."
}
```

---

### Test Runs

#### GET /api/testruns
Get all test runs.

```bash
curl -H "X-API-Key: qa-key" http://localhost:5106/api/testruns
```

---

#### GET /api/testruns/{id}
Get a specific test run.

```bash
curl -H "X-API-Key: qa-key" \
  http://localhost:5106/api/testruns/660e8400-e29b-41d4-a716-446655440001
```

---

#### POST /api/testruns
Create a new test run.

```bash
curl -X POST -H "X-API-Key: qa-key" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Sprint 3 Testing",
    "status": "InProgress",
    "executedAt": "2025-12-16T10:00:00Z",
    "testCaseIds": ["550e8400-e29b-41d4-a716-446655440001"]
  }' \
  http://localhost:5106/api/testruns
```

---

#### PUT /api/testruns/{id}
Fully update a test run.

```bash
curl -X PUT -H "X-API-Key: qa-key" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Updated name",
    "status": "Completed",
    "executedAt": null,
    "testCaseIds": []
  }' \
  http://localhost:5106/api/testruns/660e8400-e29b-41d4-a716-446655440001
```

---

#### PATCH /api/testruns/{id}
Partially update a test run.

```bash
curl -X PATCH -H "X-API-Key: qa-key" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Completed"
  }' \
  http://localhost:5106/api/testruns/660e8400-e29b-41d4-a716-446655440001
```

---

#### DELETE /api/testruns/{id}
Delete a test run.

```bash
curl -X DELETE -H "X-API-Key: qa-key" \
  http://localhost:5106/api/testruns/660e8400-e29b-41d4-a716-446655440001
```

---

### Defects

#### GET /api/defects
Get all defects.

```bash
curl -H "X-API-Key: qa-key" http://localhost:5106/api/defects
```

---

#### GET /api/defects/{id}
Get a specific defect.

```bash
curl -H "X-API-Key: qa-key" \
  http://localhost:5106/api/defects/770e8400-e29b-41d4-a716-446655440001
```

---

#### POST /api/defects
Create a new defect.

```bash
curl -X POST -H "X-API-Key: qa-key" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Login button not clickable",
    "severity": "Critical",
    "status": "Open",
    "linkedTestCaseId": "550e8400-e29b-41d4-a716-446655440001"
  }' \
  http://localhost:5106/api/defects
```

---

#### PUT /api/defects/{id}
Fully update a defect.

```bash
curl -X PUT -H "X-API-Key: qa-key" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Login button alignment broken",
    "severity": "Major",
    "status": "InProgress",
    "linkedTestCaseId": null
  }' \
  http://localhost:5106/api/defects/770e8400-e29b-41d4-a716-446655440001
```

---

#### PATCH /api/defects/{id}
Partially update a defect.

```bash
curl -X PATCH -H "X-API-Key: qa-key" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Resolved"
  }' \
  http://localhost:5106/api/defects/770e8400-e29b-41d4-a716-446655440001
```

---

#### DELETE /api/defects/{id}
Delete a defect.

```bash
curl -X DELETE -H "X-API-Key: qa-key" \
  http://localhost:5106/api/defects/770e8400-e29b-41d4-a716-446655440001
```

---

## Authentication & Authorization

### API Key Validation (401)
Missing or invalid API key:

```bash
curl http://localhost:5106/api/testcases
```

**Response (401):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.3.2",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Missing or invalid API key. Provide 'X-API-Key' header."
}
```

### Read-Only Authorization (403)
Attempting write operation with `read-key`:

```bash
curl -X POST -H "X-API-Key: read-key" \
  -H "Content-Type: application/json" \
  -d '{"title":"test"}' \
  http://localhost:5106/api/testcases
```

**Response (403):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "Your API key is read-only and cannot perform write operations."
}
```

---

## Rate Limiting

**Limit:** 10 requests per minute per API key

When exceeded:

```bash
curl -H "X-API-Key: qa-key" http://localhost:5106/api/testcases
```

**Response (429):**
```json
{
  "type": "https://tools.ietf.org/html/rfc6585#section-4",
  "title": "Too Many Requests",
  "status": 429,
  "detail": "Rate limit exceeded. Maximum 10 requests per minute."
}
```

**Headers:**
- `Retry-After: 60` - Wait 60 seconds before retrying
- `X-RateLimit-Limit: 10` - Maximum requests per minute
- `X-RateLimit-Remaining: 3` - Remaining requests in current window

---

## Postman Testing

### Import Collection
1. Open Postman
2. **File** → **Import**
3. Select `/postman/TestManagement.postman_collection.json`
4. Select **Environment** → **Import**
5. Select `/postman/local.postman_environment.json`

### Run Tests
1. Select the **local** environment
2. Click **Run** or execute individual requests
3. Tests automatically validate status codes, response schemas, and rate limiting

### Test Categories Included

1. **Test Cases:** CRUD operations, filtering, pagination, validation errors, conflict detection
2. **Test Runs:** Full lifecycle management
3. **Defects:** Creation, status tracking, severity levels
4. **Authentication:** Valid/invalid keys, read-only enforcement
5. **Error Scenarios:** 400, 401, 403, 404, 409, 429 status codes

---

## Error Codes Reference

| Status | Meaning | Example |
|--------|---------|---------|
| **200** | OK | Successful GET, POST (body), PUT, PATCH |
| **201** | Created | POST returns new resource |
| **204** | No Content | DELETE successful |
| **400** | Bad Request | Validation failed (title too short, missing required field) |
| **401** | Unauthorized | Missing or invalid API key |
| **403** | Forbidden | Read-only key attempts write operation |
| **404** | Not Found | Resource doesn't exist |
| **409** | Conflict | Cannot delete TestCase referenced by TestRun |
| **429** | Too Many Requests | Rate limit exceeded |

---

## Seed Data

The API initializes with deterministic test data:

### Sample Test Cases
- `550e8400-e29b-41d4-a716-446655440001` - "Verify login with valid credentials" (High, Active)
- `550e8400-e29b-41d4-a716-446655440002` - "Verify user registration" (High, Active)
- `550e8400-e29b-41d4-a716-446655440003` - "Verify API response time" (Medium, Active)
- `550e8400-e29b-41d4-a716-446655440004` - "Test password reset flow" (Medium, Active)
- `550e8400-e29b-41d4-a716-446655440005` - "Verify pagination on list endpoint" (Low, Inactive)

### Sample Test Runs
- `660e8400-e29b-41d4-a716-446655440001` - "Sprint 1 - Login Feature Testing" (Completed)
- `660e8400-e29b-41d4-a716-446655440002` - "Sprint 2 - Registration Feature Testing" (InProgress)
- `660e8400-e29b-41d4-a716-446655440003` - "Performance Testing Cycle 1" (Planned)

### Sample Defects
- `770e8400-e29b-41d4-a716-446655440001` - "Login button not responding on mobile" (Critical, Open)
- `770e8400-e29b-41d4-a716-446655440002` - "Email validation too strict" (Major, InProgress)
- `770e8400-e29b-41d4-a716-446655440003` - "Typo in success message" (Minor, Resolved)
- `770e8400-e29b-41d4-a716-446655440004` - "Password reset email delays" (Major, Closed)

---

## Project Structure

```
TestManagementAPI/
├── Program.cs                          # Main application entry, endpoint definitions
├── Models/
│   ├── Enums.cs                       # TestCasePriority, TestRunStatus, DefectSeverity, DefectStatus
│   ├── TestCase.cs                    # TestCase, CreateTestCaseRequest, UpdateTestCaseRequest, PatchTestCaseRequest
│   ├── TestRun.cs                     # TestRun, CreateTestRunRequest, UpdateTestRunRequest, PatchTestRunRequest
│   ├── Defect.cs                      # Defect, CreateDefectRequest, UpdateDefectRequest, PatchDefectRequest
│   └── PagedResponse.cs               # Generic pagination wrapper
├── Repositories/
│   ├── TestCaseRepository.cs          # In-memory TestCase CRUD with filtering/pagination
│   ├── TestRunRepository.cs           # In-memory TestRun CRUD
│   └── DefectRepository.cs            # In-memory Defect CRUD
├── Services/
│   ├── ApiKeyAuthenticationService.cs # API key validation
│   └── RateLimitService.cs            # Rate limiting logic
├── Middleware/
│   ├── ApiKeyAuthenticationMiddleware.cs  # API key + rate limit enforcement
│   └── RequestResponseLoggingMiddleware.cs # Request/response logging
└── Properties/
    └── launchSettings.json            # Launch configuration (port 5106)
```

---

## Interview Prep: Key Concepts Demonstrated

✅ **REST Principles:** Proper HTTP methods, status codes, resource URLs
✅ **API Design:** Filtering, pagination, error handling (ProblemDetails)
✅ **Authentication:** API key validation, read/write authorization
✅ **Rate Limiting:** Simple in-memory counter-based approach
✅ **Data Validation:** Title length constraints, required fields
✅ **Relationships:** Conflict detection (TestCase referenced by TestRun)
✅ **Testing:** Postman collection with comprehensive test scripts
✅ **Code Organization:** Models, repositories, services, middleware separation
✅ **C# Features:** Async/await, dependency injection, records, LINQ

---

## Running Postman Tests via CLI

To automate testing with Newman (Postman CLI):

```bash
npm install -g newman

newman run postman/TestManagement.postman_collection.json \
  --environment postman/local.postman_environment.json \
  --reporters cli,html \
  --reporter-html-export test-results.html
```

---

## Logging

Request/response logs are output to the console during execution:

```
info: TestManagementAPI
      Request: { Timestamp = 2025-12-16T10:50:23.1234567Z, Method = GET, Path = /api/testcases, QueryString = ?page=1&pageSize=10, ... }
info: TestManagementAPI
      Response: { Timestamp = 2025-12-16T10:50:23.1564789Z, StatusCode = 200, Method = GET, Path = /api/testcases }
```

---

## Notes for QA/Interview

1. **Pagination** is implemented on TestCases endpoint as requested; similar patterns can be applied to other resources
2. **PATCH** uses a simple partial-update model where only provided fields are updated; JSON Patch (RFC 6902) can be substituted for production
3. **Rate limiting** uses in-memory timestamps; production would use Redis or similar
4. **Seed data** uses hard-coded GUIDs for predictable testing
5. **Swagger** integration allows exploring all endpoints visually
6. All responses follow **ProblemDetails** (RFC 7807) for consistency

---

## License

This is a learning/practice project created for QA interview preparation.

