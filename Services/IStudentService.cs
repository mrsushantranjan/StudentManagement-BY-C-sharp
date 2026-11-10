using StudentManagement.DTOs;

namespace StudentManagement.Services;

/// <summary>
/// Business logic contract for managing students.
/// </summary>
public interface IStudentService
{
    Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(
        StudentQueryParameters query,
        CancellationToken cancellationToken = default);

    Task<StudentResponseDto> GetStudentByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<StudentResponseDto> CreateStudentAsync(
        CreateStudentRequest request,
        CancellationToken cancellationToken = default);

    Task<StudentResponseDto> UpdateStudentAsync(
        int id,
        UpdateStudentRequest request,
        CancellationToken cancellationToken = default);

    Task<StudentResponseDto> UpdateStudentStatusAsync(
        int id,
        UpdateStudentStatusRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteStudentAsync(int id, CancellationToken cancellationToken = default);

    Task<StudentStatsDto> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

