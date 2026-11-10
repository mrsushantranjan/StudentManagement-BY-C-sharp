using StudentManagement.Models;

namespace StudentManagement.Repositories;

/// <summary>
/// Data access abstraction for student records.
/// </summary>
public interface IStudentRepository
{
    Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, int? excludeStudentId = null, CancellationToken cancellationToken = default);
    Task<bool> EnrollmentNumberExistsAsync(string enrollmentNumber, int? excludeStudentId = null, CancellationToken cancellationToken = default);
    Task<Student> AddAsync(Student student, CancellationToken cancellationToken = default);
    Task<Student?> UpdateAsync(Student student, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

