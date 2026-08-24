using System.Security.Claims;
using jira_lite.Data;
using jira_lite.DTOs;
using jira_lite.Models;
using jira_lite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace jira_lite.Controllers;

// Depth limit: Story(0) → Task(1) → Subtask(2). Subtask has no children — max depth enforced by the model.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubtasksController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProjectRoleService _roleService;

    public SubtasksController(AppDbContext db, ProjectRoleService roleService)
    {
        _db = db;
        _roleService = roleService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async System.Threading.Tasks.Task<int?> GetProjectId(int taskId)
    {
        var task = await _db.Tasks
            .Include(t => t.Story).ThenInclude(s => s.Epic)
            .FirstOrDefaultAsync(t => t.Id == taskId);
        return task?.Story.Epic.ProjectId;
    }

    // GET /api/subtasks?taskId=1
    [HttpGet]
    public async System.Threading.Tasks.Task<IActionResult> GetAll([FromQuery] int taskId)
    {
        var userId = GetUserId();
        var projectId = await GetProjectId(taskId);
        if (projectId is null) return NotFound(new { message = "Task not found." });

        if (!await _roleService.HasAnyAccessAsync(userId, projectId.Value))
            return Forbid();

        var subtasks = await _db.Subtasks
            .Include(s => s.Status)
            .Include(s => s.CreatedBy)
            .Include(s => s.Assignee)
            .Include(s => s.Task)
            .Where(s => s.TaskId == taskId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SubtaskDto
            {
                Id            = s.Id,
                Title         = s.Title,
                Description   = s.Description,
                Priority      = s.Priority,
                CreatedAt     = s.CreatedAt,
                UpdatedAt     = s.UpdatedAt,
                TaskId        = s.TaskId,
                TaskTitle     = s.Task.Title,
                StatusId      = s.StatusId,
                StatusName    = s.Status.Name,
                StatusColor   = s.Status.Color ?? string.Empty,
                CreatedById   = s.CreatedById,
                CreatedByName = s.CreatedBy.FullName,
                AssigneeId    = s.AssigneeId,
                AssigneeName  = s.Assignee != null ? s.Assignee.FullName : null
            })
            .ToListAsync();

        return Ok(subtasks);
    }

    // GET /api/subtasks/{id}
    [HttpGet("{id}")]
    public async System.Threading.Tasks.Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();

        var s = await _db.Subtasks
            .Include(s => s.Status)
            .Include(s => s.CreatedBy)
            .Include(s => s.Assignee)
            .Include(s => s.Task).ThenInclude(t => t.Story).ThenInclude(st => st.Epic)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (s is null) return NotFound();

        if (!await _roleService.HasAnyAccessAsync(userId, s.Task.Story.Epic.ProjectId))
            return Forbid();

        return Ok(new SubtaskDto
        {
            Id            = s.Id,
            Title         = s.Title,
            Description   = s.Description,
            Priority      = s.Priority,
            CreatedAt     = s.CreatedAt,
            UpdatedAt     = s.UpdatedAt,
            TaskId        = s.TaskId,
            TaskTitle     = s.Task.Title,
            StatusId      = s.StatusId,
            StatusName    = s.Status.Name,
            StatusColor   = s.Status.Color ?? string.Empty,
            CreatedById   = s.CreatedById,
            CreatedByName = s.CreatedBy.FullName,
            AssigneeId    = s.AssigneeId,
            AssigneeName  = s.Assignee?.FullName
        });
    }

    // POST /api/subtasks
    [HttpPost]
    public async System.Threading.Tasks.Task<IActionResult> Create([FromBody] CreateSubtaskRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        var projectId = await GetProjectId(request.TaskId);
        if (projectId is null) return NotFound(new { message = "Task not found." });

        if (!await _roleService.IsMemberOrAboveAsync(userId, projectId.Value))
            return Forbid();

        var subtask = new Subtask
        {
            Title       = request.Title,
            Description = request.Description,
            Priority    = request.Priority,
            TaskId      = request.TaskId,
            StatusId    = 1,
            CreatedById = userId,
            AssigneeId  = request.AssigneeId,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        };

        _db.Subtasks.Add(subtask);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = subtask.Id }, new { id = subtask.Id });
    }

    // PUT /api/subtasks/{id}
    [HttpPut("{id}")]
    public async System.Threading.Tasks.Task<IActionResult> Update(int id, [FromBody] UpdateSubtaskRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();

        var subtask = await _db.Subtasks
            .Include(s => s.Task).ThenInclude(t => t.Story).ThenInclude(st => st.Epic)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (subtask is null) return NotFound();

        if (!await _roleService.IsMemberOrAboveAsync(userId, subtask.Task.Story.Epic.ProjectId))
            return Forbid();

        if (!await _db.WorkflowStatuses.AnyAsync(s => s.Id == request.StatusId))
            return BadRequest(new { message = "Invalid status." });

        subtask.Title       = request.Title;
        subtask.Description = request.Description;
        subtask.Priority    = request.Priority;
        subtask.AssigneeId  = request.AssigneeId;
        subtask.StatusId    = request.StatusId;
        subtask.UpdatedAt   = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/subtasks/{id}
    [HttpDelete("{id}")]
    public async System.Threading.Tasks.Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var subtask = await _db.Subtasks
            .Include(s => s.Task).ThenInclude(t => t.Story).ThenInclude(st => st.Epic)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (subtask is null) return NotFound();

        if (!await _roleService.IsLeadOrAboveAsync(userId, subtask.Task.Story.Epic.ProjectId))
            return Forbid();

        _db.Subtasks.Remove(subtask);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
