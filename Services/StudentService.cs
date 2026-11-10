using StudentManagement.DTOs;
using StudentManagement.Exceptions;
using StudentManagement.Models;
using StudentManagement.Repositories;

namespace StudentManagement.Services;

/// <summary>
/// Implements validation, filtering, sorting, and persistence orchestration for students.
/// </summary>
public sealed class StudentService(IStudentRepository repository) : IStudentService
{
    public async Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(
        StudentQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Student> students = await repository.GetAllAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            students = students.Where(s =>
                s.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                s.LastName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                s.Email.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                s.EnrollmentNumber.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            var dept = query.Department.Trim();
            students = students.Where(s =>
                string.Equals(s.Department, dept, StringComparison.OrdinalIgnoreCase));
        }

        if (query.IsActive.HasValue)
        {
            students = students.Where(s => s.IsActive == query.IsActive.Value);
        }

        students = ApplySorting(students, query.SortBy, query.SortOrder);

        var filteredList = students.ToList();
        var totalCount = filteredList.Count;
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var items = filteredList
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(MapToDto)
            .ToList();

        return new PagedResponse<StudentResponseDto>(
            items,
            query.Page,
            query.PageSize,
            totalCount,
            totalPages);
    }

    public async Task<StudentResponseDto> GetStudentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var student = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Student with ID {id} was not found.");

        return MapToDto(student);
    }

    public async Task<StudentResponseDto> CreateStudentAsync(
        CreateStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedEnrollment = request.EnrollmentNumber.Trim().ToUpperInvariant();

        await EnsureUniqueIdentifiersAsync(normalizedEmail, normalizedEnrollment, null, cancellationToken);

        var student = new Student
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            EnrollmentNumber = normalizedEnrollment,
            Department = request.Department.Trim(),
            Semester = request.Semester,
            Gpa = request.Gpa,
            EnrollmentDate = request.EnrollmentDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true
        };

        var created = await repository.AddAsync(student, cancellationToken);
        return MapToDto(created);
    }

    public async Task<StudentResponseDto> UpdateStudentAsync(
        int id,
        UpdateStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Student with ID {id} was not found.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedEnrollment = request.EnrollmentNumber.Trim().ToUpperInvariant();

        await EnsureUniqueIdentifiersAsync(normalizedEmail, normalizedEnrollment, id, cancellationToken);

        existing.FirstName = request.FirstName.Trim();
        existing.LastName = request.LastName.Trim();
        existing.Email = normalizedEmail;
        existing.EnrollmentNumber = normalizedEnrollment;
        existing.Department = request.Department.Trim();
        existing.Semester = request.Semester;
        existing.Gpa = request.Gpa;
        existing.EnrollmentDate = request.EnrollmentDate;
        existing.IsActive = request.IsActive;

        var updated = await repository.UpdateAsync(existing, cancellationToken)
            ?? throw new NotFoundException($"Student with ID {id} was not found.");

        return MapToDto(updated);
    }

    public async Task<StudentResponseDto> UpdateStudentStatusAsync(
        int id,
        UpdateStudentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Student with ID {id} was not found.");

        existing.IsActive = request.IsActive;

        var updated = await repository.UpdateAsync(existing, cancellationToken)
            ?? throw new NotFoundException($"Student with ID {id} was not found.");

        return MapToDto(updated);
    }

    public async Task DeleteStudentAsync(int id, CancellationToken cancellationToken = default)
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Student with ID {id} was not found.");
        }
    }

    public async Task<StudentStatsDto> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var students = await repository.GetAllAsync(cancellationToken);
        var total = students.Count;
        var active = students.Count(s => s.IsActive);
        var avgGpa = total == 0 ? 0m : Math.Round(students.Average(s => s.Gpa), 2);

        var byDepartment = students
            .GroupBy(s => s.Department, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.First().Department, g => g.Count());

        return new StudentStatsDto(
            TotalStudents: total,
            ActiveStudents: active,
            InactiveStudents: total - active,
            AverageGpa: avgGpa,
            StudentsByDepartment: byDepartment);
    }

    private async Task EnsureUniqueIdentifiersAsync(
        string email,
        string enrollmentNumber,
        int? excludeStudentId,
        CancellationToken cancellationToken)
    {
        if (await repository.EmailExistsAsync(email, excludeStudentId, cancellationToken))
        {
            throw new ConflictException($"A student with email '{email}' already exists.");
        }

        if (await repository.EnrollmentNumberExistsAsync(enrollmentNumber, excludeStudentId, cancellationToken))
        {
            throw new ConflictException($"A student with enrollment number '{enrollmentNumber}' already exists.");
        }
    }

    private static IEnumerable<Student> ApplySorting(
        IEnumerable<Student> students,
        string sortBy,
        string sortOrder)
    {
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLowerInvariant() switch
        {
            "firstname" => descending ? students.OrderByDescending(s => s.FirstName) : students.OrderBy(s => s.FirstName),
            "lastname" => descending ? students.OrderByDescending(s => s.LastName) : students.OrderBy(s => s.LastName),
            "email" => descending ? students.OrderByDescending(s => s.Email) : students.OrderBy(s => s.Email),
            "department" => descending ? students.OrderByDescending(s => s.Department) : students.OrderBy(s => s.Department),
            "semester" => descending ? students.OrderByDescending(s => s.Semester) : students.OrderBy(s => s.Semester),
            "gpa" => descending ? students.OrderByDescending(s => s.Gpa) : students.OrderBy(s => s.Gpa),
            "enrollmentdate" => descending ? students.OrderByDescending(s => s.EnrollmentDate) : students.OrderBy(s => s.EnrollmentDate),
            _ => descending ? students.OrderByDescending(s => s.Id) : students.OrderBy(s => s.Id)
        };
    }

    private static StudentResponseDto MapToDto(Student student) => new(
        student.Id,
        student.FirstName,
        student.LastName,
        $"{student.FirstName} {student.LastName}",
        student.Email,
        student.EnrollmentNumber,
        student.Department,
        student.Semester,
        student.Gpa,
        student.EnrollmentDate,
        student.IsActive,
        student.CreatedAtUtc,
        student.UpdatedAtUtc);
}

