using System.Collections.Concurrent;
using StudentManagement.Models;

namespace StudentManagement.Repositories;

/// <summary>
/// Thread-safe in-memory repository seeded with sample student records.
/// </summary>
public sealed class InMemoryStudentRepository : IStudentRepository
{
    private readonly ConcurrentDictionary<int, Student> _students = new();
    private int _nextId;

    public InMemoryStudentRepository()
    {
        SeedData();
    }

    public Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<Student> snapshot = _students.Values
            .Select(Clone)
            .OrderBy(s => s.Id)
            .ToList();
        return Task.FromResult(snapshot);
    }

    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_students.TryGetValue(id, out var student) ? Clone(student) : null);
    }

    public Task<bool> EmailExistsAsync(
        string email,
        int? excludeStudentId = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var exists = _students.Values.Any(s =>
            s.Id != excludeStudentId &&
            string.Equals(s.Email, email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }

    public Task<bool> EnrollmentNumberExistsAsync(
        string enrollmentNumber,
        int? excludeStudentId = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var exists = _students.Values.Any(s =>
            s.Id != excludeStudentId &&
            string.Equals(s.EnrollmentNumber, enrollmentNumber, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }

    public Task<Student> AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTime.UtcNow;
        var created = Clone(student);
        created.Id = Interlocked.Increment(ref _nextId);
        created.CreatedAtUtc = now;
        created.UpdatedAtUtc = now;

        _students[created.Id] = created;
        return Task.FromResult(Clone(created));
    }

    public Task<Student?> UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_students.TryGetValue(student.Id, out var existing))
        {
            return Task.FromResult<Student?>(null);
        }

        var updated = Clone(student);
        updated.CreatedAtUtc = existing.CreatedAtUtc;
        updated.UpdatedAtUtc = DateTime.UtcNow;

        _students[updated.Id] = updated;
        return Task.FromResult<Student?>(Clone(updated));
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_students.TryRemove(id, out _));
    }

    private void SeedData()
    {
        var now = DateTime.UtcNow;
        var initialStudents = new[]
        {
            new Student
            {
                FirstName = "Aarav",
                LastName = "Sharma",
                Email = "aarav.sharma@university.edu",
                EnrollmentNumber = "CS2024001",
                Department = "Computer Science",
                Semester = 5,
                Gpa = 8.95m,
                EnrollmentDate = new DateOnly(2024, 8, 1),
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            new Student
            {
                FirstName = "Priya",
                LastName = "Nair",
                Email = "priya.nair@university.edu",
                EnrollmentNumber = "EC2024014",
                Department = "Electronics",
                Semester = 5,
                Gpa = 9.20m,
                EnrollmentDate = new DateOnly(2024, 8, 1),
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            new Student
            {
                FirstName = "Rohan",
                LastName = "Verma",
                Email = "rohan.verma@university.edu",
                EnrollmentNumber = "CS2023042",
                Department = "Computer Science",
                Semester = 7,
                Gpa = 8.10m,
                EnrollmentDate = new DateOnly(2023, 8, 5),
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            },
            new Student
            {
                FirstName = "Meera",
                LastName = "Kulkarni",
                Email = "meera.kulkarni@university.edu",
                EnrollmentNumber = "ME2022019",
                Department = "Mechanical",
                Semester = 8,
                Gpa = 7.85m,
                EnrollmentDate = new DateOnly(2022, 8, 10),
                IsActive = false,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            }
        };

        foreach (var student in initialStudents)
        {
            student.Id = Interlocked.Increment(ref _nextId);
            _students[student.Id] = student;
        }
    }

    private static Student Clone(Student source) => new()
    {
        Id = source.Id,
        FirstName = source.FirstName,
        LastName = source.LastName,
        Email = source.Email,
        EnrollmentNumber = source.EnrollmentNumber,
        Department = source.Department,
        Semester = source.Semester,
        Gpa = source.Gpa,
        EnrollmentDate = source.EnrollmentDate,
        IsActive = source.IsActive,
        CreatedAtUtc = source.CreatedAtUtc,
        UpdatedAtUtc = source.UpdatedAtUtc
    };
}

