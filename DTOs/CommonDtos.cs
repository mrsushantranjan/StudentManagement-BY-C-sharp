using System.ComponentModel.DataAnnotations;

namespace StudentManagement.DTOs;

/// <summary>
/// Query parameters for filtering, sorting, and paginating student lists.
/// </summary>
public sealed class StudentQueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    public string? Search { get; init; }
    public string? Department { get; init; }
    public bool? IsActive { get; init; }
    public string SortBy { get; init; } = "id";
    public string SortOrder { get; init; } = "asc";

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, MaxPageSize)]
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = Math.Clamp(value, 1, MaxPageSize);
    }
}

/// <summary>
/// Standardized paginated collection wrapper.
/// </summary>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

/// <summary>
/// Aggregated statistics across student records.
/// </summary>
public sealed record StudentStatsDto(
    int TotalStudents,
    int ActiveStudents,
    int InactiveStudents,
    decimal AverageGpa,
    IReadOnlyDictionary<string, int> StudentsByDepartment);

