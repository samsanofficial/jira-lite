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
public class StoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProjectRoleService _roleService;

    public StoriesController(AppDbContext db, ProjectRoleService roleService)
    {
        _db = db;
        _roleService = roleService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /api/stories?epicId=1
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int epicId)
    {
        var userId = GetUserId();

        var epic = await _db.Epics.FindAsync(epicId);
        if (epic is null) return NotFound(new { message = "Epic not found." });

        if (!await _roleService.HasAnyAccessAsync(userId, epic.ProjectId))
            return Forbid();

        var stories = await _db.Stories
            .Include(s => s.Status)
            .Include(s => s.CreatedBy)
            .Include(s => s.Assignee)
            .Include(s => s.Epic)
            .Include(s => s.Tasks)
            .Where(s => s.EpicId == epicId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new StoryDto
            {
                Id            = s.Id,
                Title         = s.Title,
                Description   = s.Description,
                Priority      = s.Priority,
                StoryPoints   = s.StoryPoints,
                CreatedAt     = s.CreatedAt,
                UpdatedAt     = s.UpdatedAt,
                EpicId        = s.EpicId,
                EpicTitle     = s.Epic.Title,
                ProjectId     = s.Epic.ProjectId,
                StatusId      = s.StatusId,
                StatusName    = s.Status.Name,
                StatusColor   = s.Status.Color ?? string.Empty,
                CreatedById   = s.CreatedById,
                CreatedByName = s.CreatedBy.FullName,
                AssigneeId    = s.AssigneeId,
                AssigneeName  = s.Assignee != null ? s.Assignee.FullName : null,
                TaskCount     = s.Tasks.Count
            })
            .ToListAsync();

        return Ok(stories);
    }

    // GET /api/stories/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();

        var story = await _db.Stories
            .Include(s => s.Status)
            .Include(s => s.CreatedBy)
            .Include(s => s.Assignee)
            .Include(s => s.Epic)
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (story is null) return NotFound();

        if (!await _roleService.HasAnyAccessAsync(userId, story.Epic.ProjectId))
            return Forbid();

        return Ok(new StoryDto
        {
            Id            = story.Id,
            Title         = story.Title,
            Description   = story.Description,
            Priority      = story.Priority,
            StoryPoints   = story.StoryPoints,
            CreatedAt     = story.CreatedAt,
            UpdatedAt     = story.UpdatedAt,
            EpicId        = story.EpicId,
            EpicTitle     = story.Epic.Title,
            ProjectId     = story.Epic.ProjectId,
            StatusId      = story.StatusId,
            StatusName    = story.Status.Name,
            StatusColor   = story.Status.Color ?? string.Empty,
            CreatedById   = story.CreatedById,
            CreatedByName = story.CreatedBy.FullName,
            AssigneeId    = story.AssigneeId,
            AssigneeName  = story.Assignee?.FullName,
            TaskCount     = story.Tasks.Count
        });
    }

    // POST /api/stories
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStoryRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();

        var epic = await _db.Epics.FindAsync(request.EpicId);
        if (epic is null) return NotFound(new { message = "Epic not found." });

        if (!await _roleService.IsMemberOrAboveAsync(userId, epic.ProjectId))
            return Forbid();

        var story = new Story
        {
            Title       = request.Title,
            Description = request.Description,
            Priority    = request.Priority,
            StoryPoints = request.StoryPoints,
            EpicId      = request.EpicId,
            StatusId    = 1,
            CreatedById = userId,
            AssigneeId  = request.AssigneeId,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        };

        _db.Stories.Add(story);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = story.Id }, new { id = story.Id });
    }

    // PUT /api/stories/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStoryRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();

        var story = await _db.Stories.Include(s => s.Epic).FirstOrDefaultAsync(s => s.Id == id);
        if (story is null) return NotFound();

        if (!await _roleService.IsMemberOrAboveAsync(userId, story.Epic.ProjectId))
            return Forbid();

        var statusExists = await _db.WorkflowStatuses.AnyAsync(s => s.Id == request.StatusId);
        if (!statusExists) return BadRequest(new { message = "Invalid status." });

        story.Title       = request.Title;
        story.Description = request.Description;
        story.Priority    = request.Priority;
        story.StoryPoints = request.StoryPoints;
        story.AssigneeId  = request.AssigneeId;
        story.StatusId    = request.StatusId;
        story.UpdatedAt   = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/stories/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var story = await _db.Stories.Include(s => s.Epic).FirstOrDefaultAsync(s => s.Id == id);
        if (story is null) return NotFound();

        if (!await _roleService.IsLeadOrAboveAsync(userId, story.Epic.ProjectId))
            return Forbid();

        _db.Stories.Remove(story);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
