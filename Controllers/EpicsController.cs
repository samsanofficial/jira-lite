using System.Security.Claims;
using jira_lite.Data;
using jira_lite.DTOs;
using jira_lite.Models;
using jira_lite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace jira_lite.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EpicsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProjectRoleService _roleService;

    public EpicsController(AppDbContext db, ProjectRoleService roleService)
    {
        _db = db;
        _roleService = roleService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /api/epics?projectId=1
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int projectId)
    {
        var userId = GetUserId();

        if (!await _roleService.HasAnyAccessAsync(userId, projectId))
            return Forbid();

        var epics = await _db.Epics
            .Include(e => e.Status)
            .Include(e => e.CreatedBy)
            .Include(e => e.Assignee)
            .Include(e => e.Stories)
            .Where(e => e.ProjectId == projectId)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new EpicDto
            {
                Id            = e.Id,
                Title         = e.Title,
                Description   = e.Description,
                Priority      = e.Priority,
                DueDate       = e.DueDate,
                CreatedAt     = e.CreatedAt,
                UpdatedAt     = e.UpdatedAt,
                ProjectId     = e.ProjectId,
                StatusId      = e.StatusId,
                StatusName    = e.Status.Name,
                StatusColor   = e.Status.Color ?? string.Empty,
                CreatedById   = e.CreatedById,
                CreatedByName = e.CreatedBy.FullName,
                AssigneeId    = e.AssigneeId,
                AssigneeName  = e.Assignee != null ? e.Assignee.FullName : null,
                StoryCount    = e.Stories.Count
            })
            .ToListAsync();

        return Ok(epics);
    }

    // GET /api/epics/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();

        var epic = await _db.Epics
            .Include(e => e.Status)
            .Include(e => e.CreatedBy)
            .Include(e => e.Assignee)
            .Include(e => e.Stories)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (epic is null) return NotFound();

        if (!await _roleService.HasAnyAccessAsync(userId, epic.ProjectId))
            return Forbid();

        return Ok(new EpicDto
        {
            Id            = epic.Id,
            Title         = epic.Title,
            Description   = epic.Description,
            Priority      = epic.Priority,
            DueDate       = epic.DueDate,
            CreatedAt     = epic.CreatedAt,
            UpdatedAt     = epic.UpdatedAt,
            ProjectId     = epic.ProjectId,
            StatusId      = epic.StatusId,
            StatusName    = epic.Status.Name,
            StatusColor   = epic.Status.Color ?? string.Empty,
            CreatedById   = epic.CreatedById,
            CreatedByName = epic.CreatedBy.FullName,
            AssigneeId    = epic.AssigneeId,
            AssigneeName  = epic.Assignee?.FullName,
            StoryCount    = epic.Stories.Count
        });
    }

    // POST /api/epics
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEpicRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();

        if (!await _roleService.IsLeadOrAboveAsync(userId, request.ProjectId))
            return Forbid();

        var project = await _db.Projects.FindAsync(request.ProjectId);
        if (project is null) return NotFound(new { message = "Project not found." });

        var epic = new Epic
        {
            Title       = request.Title,
            Description = request.Description,
            Priority    = request.Priority,
            DueDate     = request.DueDate,
            ProjectId   = request.ProjectId,
            StatusId    = 1,
            CreatedById = userId,
            AssigneeId  = request.AssigneeId,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        };

        _db.Epics.Add(epic);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = epic.Id }, new { id = epic.Id });
    }

    // PUT /api/epics/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEpicRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();

        var epic = await _db.Epics.FindAsync(id);
        if (epic is null) return NotFound();

        if (!await _roleService.IsLeadOrAboveAsync(userId, epic.ProjectId))
            return Forbid();

        var statusExists = await _db.WorkflowStatuses.AnyAsync(s => s.Id == request.StatusId);
        if (!statusExists) return BadRequest(new { message = "Invalid status." });

        epic.Title       = request.Title;
        epic.Description = request.Description;
        epic.Priority    = request.Priority;
        epic.DueDate     = request.DueDate;
        epic.AssigneeId  = request.AssigneeId;
        epic.StatusId    = request.StatusId;
        epic.UpdatedAt   = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/epics/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var epic = await _db.Epics.FindAsync(id);
        if (epic is null) return NotFound();

        if (!await _roleService.IsAdminAsync(userId, epic.ProjectId))
            return Forbid();

        _db.Epics.Remove(epic);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
