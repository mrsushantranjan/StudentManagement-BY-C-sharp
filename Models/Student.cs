namespace StudentManagement.Models;

/// <summary>
/// Represents a student record in the system.
/// </summary>
public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EnrollmentNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int Semester { get; set; }
    public decimal Gpa { get; set; }
    public DateOnly EnrollmentDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

