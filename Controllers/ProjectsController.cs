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
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProjectRoleService _roleService;

    public ProjectsController(AppDbContext db, ProjectRoleService roleService)
    {
        _db = db;
        _roleService = roleService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /api/projects
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();

        var projects = await _db.Projects
            .Include(p => p.CreatedBy)
            .Where(p => p.UserProjectRoles.Any(upr => upr.UserId == userId))
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Key = p.Key,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                CreatedById = p.CreatedById,
                CreatedByName = p.CreatedBy.FullName
            })
            .ToListAsync();

        return Ok(projects);
    }

    // GET /api/projects/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();

        if (!await _roleService.HasAnyAccessAsync(userId, id))
            return Forbid();

        var project = await _db.Projects
            .Include(p => p.CreatedBy)
            .Where(p => p.Id == id)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Key = p.Key,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                CreatedById = p.CreatedById,
                CreatedByName = p.CreatedBy.FullName
            })
            .FirstOrDefaultAsync();

        if (project is null) return NotFound();

        return Ok(project);
    }

    // POST /api/projects
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        var upperKey = request.Key.ToUpper();

        if (await _db.Projects.AnyAsync(p => p.Key == upperKey))
            return Conflict(new { message = $"Project key '{upperKey}' is already in use." });

        var adminRole = await _db.Roles.FirstAsync(r => r.Name == "Project Admin");

        var project = new Project
        {
            Name        = request.Name,
            Key         = upperKey,
            Description = request.Description,
            StartDate   = request.StartDate,
            EndDate     = request.EndDate,
            CreatedById = userId,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        _db.UserProjectRoles.Add(new UserProjectRole
        {
            UserId    = userId,
            ProjectId = project.Id,
            RoleId    = adminRole.Id
        });
        await _db.SaveChangesAsync();

        var creator = await _db.Users.FindAsync(userId);

        return CreatedAtAction(nameof(GetById), new { id = project.Id }, new ProjectDto
        {
            Id            = project.Id,
            Name          = project.Name,
            Key           = project.Key,
            Description   = project.Description,
            StartDate     = project.StartDate,
            EndDate       = project.EndDate,
            CreatedAt     = project.CreatedAt,
            UpdatedAt     = project.UpdatedAt,
            CreatedById   = project.CreatedById,
            CreatedByName = creator?.FullName ?? string.Empty
        });
    }

    // PUT /api/projects/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();

        if (!await _roleService.IsLeadOrAboveAsync(userId, id))
            return Forbid();

        var project = await _db.Projects.FindAsync(id);
        if (project is null) return NotFound();

        project.Name        = request.Name;
        project.Description = request.Description;
        project.StartDate   = request.StartDate;
        project.EndDate     = request.EndDate;
        project.UpdatedAt   = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /api/projects/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        if (!await _roleService.IsAdminAsync(userId, id))
            return Forbid();

        var project = await _db.Projects.FindAsync(id);
        if (project is null) return NotFound();

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
