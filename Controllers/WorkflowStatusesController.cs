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
[Route("api/projects/{projectId}/statuses")]
[Authorize]
public class WorkflowStatusesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProjectRoleService _roleService;

    public WorkflowStatusesController(AppDbContext db, ProjectRoleService roleService)
    {
        _db = db;
        _roleService = roleService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /api/projects/{projectId}/statuses
    [HttpGet]
    public async Task<IActionResult> GetAll(int projectId)
    {
        var userId = GetUserId();
        if (!await _roleService.HasAnyAccessAsync(userId, projectId)) return Forbid();

        var statuses = await _db.WorkflowStatuses
            .Where(ws => ws.ProjectId == projectId)
            .OrderBy(ws => ws.Order)
            .Select(ws => new WorkflowStatusDto
            {
                Id = ws.Id, Name = ws.Name, Color = ws.Color,
                Order = ws.Order, ProjectId = ws.ProjectId, IsGlobal = false
            })
            .ToListAsync();

        // Fall back to global defaults when project has no custom statuses
        if (statuses.Count == 0)
        {
            statuses = await _db.WorkflowStatuses
                .Where(ws => ws.ProjectId == null)
                .OrderBy(ws => ws.Order)
                .Select(ws => new WorkflowStatusDto
                {
                    Id = ws.Id, Name = ws.Name, Color = ws.Color,
                    Order = ws.Order, ProjectId = null, IsGlobal = true
                })
                .ToListAsync();
        }

        return Ok(statuses);
    }

    // POST /api/projects/{projectId}/statuses
    [HttpPost]
    public async Task<IActionResult> Create(int projectId, [FromBody] CreateWorkflowStatusRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        if (!await _roleService.IsLeadOrAboveAsync(userId, projectId)) return Forbid();

        if (await _db.Projects.FindAsync(projectId) is null) return NotFound();

        var maxOrder = await _db.WorkflowStatuses
            .Where(ws => ws.ProjectId == projectId)
            .MaxAsync(ws => (int?)ws.Order) ?? 0;

        var status = new WorkflowStatus
        {
            Name      = request.Name,
            Color     = string.IsNullOrEmpty(request.Color) ? "#cccccc" : request.Color,
            Order     = request.Order > 0 ? request.Order : maxOrder + 1,
            ProjectId = projectId
        };

        _db.WorkflowStatuses.Add(status);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { projectId }, new WorkflowStatusDto
        {
            Id = status.Id, Name = status.Name, Color = status.Color,
            Order = status.Order, ProjectId = status.ProjectId, IsGlobal = false
        });
    }

    // PUT /api/projects/{projectId}/statuses/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int projectId, int id, [FromBody] UpdateWorkflowStatusRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        if (!await _roleService.IsLeadOrAboveAsync(userId, projectId)) return Forbid();

        var status = await _db.WorkflowStatuses
            .FirstOrDefaultAsync(ws => ws.Id == id && ws.ProjectId == projectId);
        if (status is null) return NotFound();

        status.Name  = request.Name;
        status.Color = string.IsNullOrEmpty(request.Color) ? status.Color : request.Color;
        status.Order = request.Order;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/projects/{projectId}/statuses/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int projectId, int id)
    {
        var userId = GetUserId();
        if (!await _roleService.IsAdminAsync(userId, projectId)) return Forbid();

        var status = await _db.WorkflowStatuses
            .FirstOrDefaultAsync(ws => ws.Id == id && ws.ProjectId == projectId);
        if (status is null) return NotFound();

        var inUse = await _db.Epics.AnyAsync(e => e.StatusId == id)
                 || await _db.Stories.AnyAsync(s => s.StatusId == id)
                 || await _db.Tasks.AnyAsync(t => t.StatusId == id)
                 || await _db.Subtasks.AnyAsync(st => st.StatusId == id);

        if (inUse)
            return Conflict(new { message = "Cannot delete a status that is currently assigned to existing items." });

        _db.WorkflowStatuses.Remove(status);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
