# StudentManagement-BY-C-sharp

Student Management REST API built with ASP.NET Core (`.NET 10`) and C# using a layered **Controller–Service–Repository** architecture, custom middleware pipeline, DTO validation, and a responsive static web UI.

## Features

- **RESTful API (`api/students`, `api/health`, `/api`)**
  - Full CRUD (`GET`, `POST`, `PUT`, `PATCH`, `DELETE`)
  - Search, department/status filtering, sorting, and pagination (`PagedResponse<T>`)
  - Aggregated statistics (`GET /api/students/stats`)
  - Duplicate email and enrollment number conflict detection (`409 Conflict`)
  - DataAnnotations request validation (`400 ValidationProblemDetails`)
- **Custom Middleware Pipeline**
  - `CorrelationIdMiddleware`: Reads or generates `X-Correlation-ID` headers for request tracing
  - `RequestLoggingMiddleware`: Logs request/response metadata and adds `X-Response-Time-Ms`
  - `ExceptionHandlingMiddleware`: Translates `ApiException` and unhandled errors into RFC 7807 `ProblemDetails` JSON
- **Layered Project Structure**
  - `Controllers/` — API endpoints (`StudentsController`, `HealthController`)
  - `DTOs/` — Request/response contracts and query parameters
  - `Exceptions/` — Domain-to-HTTP exception types
  - `Middleware/` — Pipeline middleware and registration extensions
  - `Models/` — Domain entity (`Student`)
  - `Repositories/` — `IStudentRepository` and thread-safe `InMemoryStudentRepository`
  - `Services/` — `IStudentService` and `StudentService`
  - `wwwroot/` — Responsive dashboard UI (`index.html`, `css/styles.css`, `js/app.js`)

## Run Locally

```bash
dotnet run
```

Open:
- `http://localhost:5165/` — Student Management Web UI
- `http://localhost:5165/api` — API route discovery JSON
- `http://localhost:5165/api/students` — Students endpoint

You can also run sample requests from `StudentManagement.http`.
