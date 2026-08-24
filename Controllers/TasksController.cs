using System.Security.Claims;
using jira_lite.Data;
using jira_lite.DTOs;
using jira_lite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelTask = jira_lite.Models.Task;

namespace jira_lite.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProjectRoleService _roleService;

    public TasksController(AppDbContext db, ProjectRoleService roleService)
    {
        _db = db;
        _roleService = roleService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async System.Threading.Tasks.Task<int?> GetProjectId(int storyId)
    {
        var story = await _db.Stories
            .Include(s => s.Epic)
            .FirstOrDefaultAsync(s => s.Id == storyId);
        return story?.Epic.ProjectId;
    }

    // GET /api/tasks?storyId=1
    [HttpGet]
    public async System.Threading.Tasks.Task<IActionResult> GetAll([FromQuery] int storyId)
    {
        var userId = GetUserId();
        var projectId = await GetProjectId(storyId);
        if (projectId is null) return NotFound(new { message = "Story not found." });

        if (!await _roleService.HasAnyAccessAsync(userId, projectId.Value))
            return Forbid();

        var tasks = await _db.Tasks
            .Include(t => t.Status)
            .Include(t => t.CreatedBy)
            .Include(t => t.Assignee)
            .Include(t => t.Story).ThenInclude(s => s.Epic)
            .Include(t => t.Subtasks)
            .Where(t => t.StoryId == storyId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TaskDto
            {
                Id             = t.Id,
                Title          = t.Title,
                Description    = t.Description,
                Priority       = t.Priority,
                EstimatedHours = t.EstimatedHours,
                LoggedHours    = t.LoggedHours,
                CreatedAt      = t.CreatedAt,
                UpdatedAt      = t.UpdatedAt,
                StoryId        = t.StoryId,
                StoryTitle     = t.Story.Title,
                EpicId         = t.Story.EpicId,
                ProjectId      = t.Story.Epic.ProjectId,
                StatusId       = t.StatusId,
                StatusName     = t.Status.Name,
                StatusColor    = t.Status.Color ?? string.Empty,
                CreatedById    = t.CreatedById,
                CreatedByName  = t.CreatedBy.FullName,
                AssigneeId     = t.AssigneeId,
                AssigneeName   = t.Assignee != null ? t.Assignee.FullName : null,
                SubtaskCount   = t.Subtasks.Count
            })
            .ToListAsync();

        return Ok(tasks);
    }

    // GET /api/tasks/{id}
    [HttpGet("{id}")]
    public async System.Threading.Tasks.Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();

        var t = await _db.Tasks
            .Include(t => t.Status)
            .Include(t => t.CreatedBy)
            .Include(t => t.Assignee)
            .Include(t => t.Story).ThenInclude(s => s.Epic)
            .Include(t => t.Subtasks)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (t is null) return NotFound();

        if (!await _roleService.HasAnyAccessAsync(userId, t.Story.Epic.ProjectId))
            return Forbid();

        return Ok(new TaskDto
        {
            Id             = t.Id,
            Title          = t.Title,
            Description    = t.Description,
            Priority       = t.Priority,
            EstimatedHours = t.EstimatedHours,
            LoggedHours    = t.LoggedHours,
            CreatedAt      = t.CreatedAt,
            UpdatedAt      = t.UpdatedAt,
            StoryId        = t.StoryId,
            StoryTitle     = t.Story.Title,
            EpicId         = t.Story.EpicId,
            ProjectId      = t.Story.Epic.ProjectId,
            StatusId       = t.StatusId,
            StatusName     = t.Status.Name,
            StatusColor    = t.Status.Color ?? string.Empty,
            CreatedById    = t.CreatedById,
            CreatedByName  = t.CreatedBy.FullName,
            AssigneeId     = t.AssigneeId,
            AssigneeName   = t.Assignee?.FullName,
            SubtaskCount   = t.Subtasks.Count
        });
    }

    // POST /api/tasks
    [HttpPost]
    public async System.Threading.Tasks.Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        var projectId = await GetProjectId(request.StoryId);
        if (projectId is null) return NotFound(new { message = "Story not found." });

        if (!await _roleService.IsMemberOrAboveAsync(userId, projectId.Value))
            return Forbid();

        var entity = new ModelTask
        {
            Title          = request.Title,
            Description    = request.Description,
            Priority       = request.Priority,
            EstimatedHours = request.EstimatedHours,
            StoryId        = request.StoryId,
            StatusId       = 1,
            CreatedById    = userId,
            AssigneeId     = request.AssigneeId,
            CreatedAt      = DateTime.UtcNow,
            UpdatedAt      = DateTime.UtcNow
        };

        _db.Tasks.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new { id = entity.Id });
    }

    // PUT /api/tasks/{id}
    [HttpPut("{id}")]
    public async System.Threading.Tasks.Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();

        var entity = await _db.Tasks
            .Include(t => t.Story).ThenInclude(s => s.Epic)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (entity is null) return NotFound();

        if (!await _roleService.IsMemberOrAboveAsync(userId, entity.Story.Epic.ProjectId))
            return Forbid();

        if (!await _db.WorkflowStatuses.AnyAsync(s => s.Id == request.StatusId))
            return BadRequest(new { message = "Invalid status." });

        entity.Title          = request.Title;
        entity.Description    = request.Description;
        entity.Priority       = request.Priority;
        entity.EstimatedHours = request.EstimatedHours;
        entity.LoggedHours    = request.LoggedHours;
        entity.AssigneeId     = request.AssigneeId;
        entity.StatusId       = request.StatusId;
        entity.UpdatedAt      = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/tasks/{id}
    [HttpDelete("{id}")]
    public async System.Threading.Tasks.Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var entity = await _db.Tasks
            .Include(t => t.Story).ThenInclude(s => s.Epic)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (entity is null) return NotFound();

        if (!await _roleService.IsLeadOrAboveAsync(userId, entity.Story.Epic.ProjectId))
            return Forbid();

        _db.Tasks.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
