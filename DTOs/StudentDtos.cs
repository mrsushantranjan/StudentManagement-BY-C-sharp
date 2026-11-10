using System.ComponentModel.DataAnnotations;

namespace StudentManagement.DTOs;

/// <summary>
/// Request payload for creating a new student.
/// </summary>
public sealed class CreateStudentRequest
{
    [Required]
    [StringLength(60, MinimumLength = 2)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [StringLength(60, MinimumLength = 2)]
    public string LastName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(30, MinimumLength = 3)]
    public string EnrollmentNumber { get; init; } = string.Empty;

    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Department { get; init; } = string.Empty;

    [Range(1, 12)]
    public int Semester { get; init; }

    [Range(0.0, 10.0)]
    public decimal Gpa { get; init; }

    public DateOnly? EnrollmentDate { get; init; }
}

/// <summary>
/// Request payload for updating an existing student.
/// </summary>
public sealed class UpdateStudentRequest
{
    [Required]
    [StringLength(60, MinimumLength = 2)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [StringLength(60, MinimumLength = 2)]
    public string LastName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(30, MinimumLength = 3)]
    public string EnrollmentNumber { get; init; } = string.Empty;

    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Department { get; init; } = string.Empty;

    [Range(1, 12)]
    public int Semester { get; init; }

    [Range(0.0, 10.0)]
    public decimal Gpa { get; init; }

    [Required]
    public DateOnly EnrollmentDate { get; init; }

    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Request payload for toggling a student's active status.
/// </summary>
public sealed class UpdateStudentStatusRequest
{
    public bool IsActive { get; init; }
}

/// <summary>
/// Response representation of a student.
/// </summary>
public sealed record StudentResponseDto(
    int Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string EnrollmentNumber,
    string Department,
    int Semester,
    decimal Gpa,
    DateOnly EnrollmentDate,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

