# Test Management API - Quick Start Summary

## What Was Created

A complete REST API for Test Management, designed for QA practice and interview preparation.

### Project Files

#### API Code (C#)
- `TestManagementAPI/Program.cs` - Main application with all 28 endpoints
- `TestManagementAPI/Models/` - Domain models (TestCase, TestRun, Defect) with request/response DTOs
- `TestManagementAPI/Repositories/` - In-memory data stores with filtering, pagination, and conflict detection
- `TestManagementAPI/Middleware/` - API key authentication, rate limiting, request/response logging
- `TestManagementAPI/Services/` - API key validation and rate limit enforcement

#### Postman Testing
- `postman/TestManagement.postman_collection.json` - Complete collection with all endpoints and test scripts
- `postman/local.postman_environment.json` - Environment variables (baseUrl, apiKey, IDs)

#### Documentation
- `README.md` - Comprehensive guide with examples, API reference, and testing instructions

---

## Running the API

```bash
cd TestManagementAPI
dotnet run
```

**URL:** http://localhost:5106
**Swagger UI:** http://localhost:5106/swagger/ui

---

## API Features Implemented

### ✅ Core Requirements

1. **Standard REST Operations**
   - GET all (with pagination)
   - GET by ID
   - POST create
   - PUT full update
   - PATCH partial update
   - DELETE remove
   - (HEAD for TestCases - checks existence)

2. **Realistic Domain**
   - TestCase: id, title (3-120 chars), description, priority (Low/Medium/High), isActive, tags array
   - TestRun: id, name, status (Planned/InProgress/Completed), executedAt, testCaseIds array
   - Defect: id, title, severity (Minor/Major/Critical), status (Open/InProgress/Resolved/Closed), linkedTestCaseId, createdAt

3. **Advanced Features**
   - Filtering: `/api/testcases?priority=High&isActive=true&tag=api`
   - Pagination: `?page=1&pageSize=10` with totalCount metadata
   - Validation: Title length, required fields, enum constraints
   - Error handling: 400 (validation), 404 (not found), 409 (conflict), 429 (rate limit)
   - Consistent ProblemDetails responses (RFC 7807)

4. **Authentication & Authorization**
   - API key header: `X-API-Key: qa-key` or `X-API-Key: read-key`
   - Full access (read/write) with `qa-key`
   - Read-only with `read-key`
   - Returns 401 if missing, 403 if unauthorized

5. **Rate Limiting**
   - 10 requests/minute per API key
   - Returns 429 with Retry-After header
   - X-RateLimit-* headers on all responses

6. **Relationships & Constraints**
   - Cannot delete TestCase if referenced by TestRun (409 Conflict)
   - Deterministic seed IDs for repeatable testing

7. **Bonus Features**
   - Request/response logging middleware
   - HEAD endpoint for existence checks
   - Swagger/OpenAPI documentation
   - In-memory repositories (zero setup)
   - Deterministic seed data

---

## Testing with Postman

### Import
1. Open Postman
2. Import collection: `postman/TestManagement.postman_collection.json`
3. Import environment: `postman/local.postman_environment.json`

### Run
- Select "Test Management API - Local" environment
- Click on requests to execute individually
- Or click "Run" to execute all with test assertions

### What's Tested
- Status codes (200, 201, 204, 400, 401, 403, 404, 409, 429)
- Response schemas
- Validation errors
- Authentication/authorization
- Read-only key restrictions
- Pagination metadata
- Conflict detection
- Rate limiting headers

---

## API Endpoints (28 Total)

### Test Cases (10 endpoints)
- GET /api/testcases
- GET /api/testcases/{id}
- POST /api/testcases
- PUT /api/testcases/{id}
- PATCH /api/testcases/{id}
- DELETE /api/testcases/{id}
- HEAD /api/testcases/{id}

### Test Runs (9 endpoints)
- GET /api/testruns
- GET /api/testruns/{id}
- POST /api/testruns
- PUT /api/testruns/{id}
- PATCH /api/testruns/{id}
- DELETE /api/testruns/{id}
- HEAD /api/testruns/{id}

### Defects (9 endpoints)
- GET /api/defects
- GET /api/defects/{id}
- POST /api/defects
- PUT /api/defects/{id}
- PATCH /api/defects/{id}
- DELETE /api/defects/{id}
- HEAD /api/defects/{id}

### Utilities (1 endpoint)
- GET /health

---

## Example Usage

### Create a Test Case
```bash
curl -X POST http://localhost:5106/api/testcases \
  -H "X-API-Key: qa-key" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test login feature",
    "priority": "High",
    "isActive": true,
    "tags": ["auth", "smoke"]
  }'
```

### Get Test Cases with Filtering
```bash
curl http://localhost:5106/api/testcases?priority=High&isActive=true&page=1&pageSize=10 \
  -H "X-API-Key: qa-key"
```

### Partially Update
```bash
curl -X PATCH http://localhost:5106/api/testcases/550e8400-e29b-41d4-a716-446655440001 \
  -H "X-API-Key: qa-key" \
  -H "Content-Type: application/json" \
  -d '{"priority": "Low"}'
```

### Delete with Conflict Handling
```bash
curl -X DELETE http://localhost:5106/api/testcases/550e8400-e29b-41d4-a716-446655440001 \
  -H "X-API-Key: qa-key"
```

Response: 409 if referenced by TestRun, 204 if successful

---

## Interview Highlights

✅ **REST Principles** - Proper HTTP methods, status codes, resource URLs
✅ **API Design Patterns** - Filtering, pagination, error handling
✅ **Authentication** - API key validation with role-based access
✅ **Rate Limiting** - Simple in-memory counter-based approach
✅ **Data Validation** - Constraints (length, required fields, enums)
✅ **Relationships** - Referential integrity (conflict on delete)
✅ **Testing** - Comprehensive Postman collection with assertions
✅ **Code Organization** - Separation of concerns (Models, Repositories, Services, Middleware)
✅ **C# Features** - Async/await, dependency injection, nullable reference types

---

## Seed Data (Hard-coded IDs)

### Test Cases
- 550e8400-e29b-41d4-a716-446655440001 - "Verify login with valid credentials"
- 550e8400-e29b-41d4-a716-446655440002 - "Verify user registration"
- 550e8400-e29b-41d4-a716-446655440003 - "Verify API response time"
- 550e8400-e29b-41d4-a716-446655440004 - "Test password reset flow"
- 550e8400-e29b-41d4-a716-446655440005 - "Verify pagination on list endpoint"

### Test Runs
- 660e8400-e29b-41d4-a716-446655440001 - "Sprint 1 - Login Feature Testing"
- 660e8400-e29b-41d4-a716-446655440002 - "Sprint 2 - Registration Feature Testing"
- 660e8400-e29b-41d4-a716-446655440003 - "Performance Testing Cycle 1"

### Defects
- 770e8400-e29b-41d4-a716-446655440001 - "Login button not responding on mobile"
- 770e8400-e29b-41d4-a716-446655440002 - "Email validation too strict"
- 770e8400-e29b-41d4-a716-446655440003 - "Typo in success message"
- 770e8400-e29b-41d4-a716-446655440004 - "Password reset email delays"

---

## Authentication Keys

- `qa-key` - Full read/write access
- `read-key` - Read-only (GET only, POST/PUT/PATCH/DELETE returns 403)

---

## Next Steps

1. **Run the API:** `cd TestManagementAPI && dotnet run`
2. **Open Swagger:** http://localhost:5106/swagger/ui
3. **Import in Postman:** Both collection and environment files in `/postman`
4. **Run Tests:** Execute requests with built-in test assertions
5. **Explore Logging:** Watch request/response logs in the console
6. **Test Rate Limiting:** Make 11+ requests within 60 seconds with same API key
7. **Test Authorization:** Try using `read-key` for POST requests

All files are ready to use. No database setup required!
