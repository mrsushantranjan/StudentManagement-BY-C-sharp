using System.Text.Json.Serialization;
using StudentManagement.Middleware;
using StudentManagement.Repositories;
using StudentManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure controllers and JSON serialization.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddProblemDetails();

// Configure CORS policy for API consumers.
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultApiCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders(CorrelationIdMiddleware.HeaderName, "X-Response-Time-Ms");
    });
});

// Register application layers via Dependency Injection.
builder.Services.AddSingleton<IStudentRepository, InMemoryStudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();

var app = builder.Build();

// Custom middleware pipeline: Correlation ID -> Request Logging -> Global Exception Handling.
app.UseApiPipelineMiddlewares();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("DefaultApiCors");
app.UseAuthorization();

app.MapControllers();

// Ignore browser favicon requests so DevTools doesn't log 404s.
app.MapGet("/favicon.ico", () => Results.NoContent());

// API discovery endpoint listing available routes.
app.MapGet("/api", () => Results.Ok(new
{
    name = "StudentManagement REST API",
    version = "1.0.0",
    endpoints = new[]
    {
        "GET    /api/health",
        "GET    /api/students",
        "GET    /api/students/{id}",
        "GET    /api/students/stats",
        "POST   /api/students",
        "PUT    /api/students/{id}",
        "PATCH  /api/students/{id}/status",
        "DELETE /api/students/{id}"
    }
}));

app.Run();
