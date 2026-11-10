using Microsoft.AspNetCore.Mvc;
using StudentManagement.DTOs;
using StudentManagement.Services;

namespace StudentManagement.Controllers;

[ApiController]
[Route("api/students")]
[Produces("application/json")]
public sealed class StudentsController(IStudentService studentService) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated, filtered, and sorted list of students.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<StudentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<StudentResponseDto>>> GetAll(
        [FromQuery] StudentQueryParameters query,
        CancellationToken cancellationToken)
    {
        var result = await studentService.GetStudentsAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single student by unique ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentResponseDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var student = await studentService.GetStudentByIdAsync(id, cancellationToken);
        return Ok(student);
    }

    /// <summary>
    /// Returns aggregated student statistics (totals, average GPA, counts by department).
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(StudentStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<StudentStatsDto>> GetStats(CancellationToken cancellationToken)
    {
        var stats = await studentService.GetStatisticsAsync(cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Creates a new student record.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StudentResponseDto>> Create(
        [FromBody] CreateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var created = await studentService.CreateStudentAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Replaces all fields of an existing student record.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StudentResponseDto>> Update(
        int id,
        [FromBody] UpdateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await studentService.UpdateStudentAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Updates the active status of an existing student.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentResponseDto>> UpdateStatus(
        int id,
        [FromBody] UpdateStudentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await studentService.UpdateStudentStatusAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a student record by ID.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await studentService.DeleteStudentAsync(id, cancellationToken);
        return NoContent();
    }
}

