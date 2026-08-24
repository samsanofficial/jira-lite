using System.Diagnostics;
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

    // GET /api/projects/{id}/tree
    [HttpGet("{id}/tree")]
    public async Task<IActionResult> GetTree(int id)
    {
        var userId = GetUserId();

        if (!await _roleService.HasAnyAccessAsync(userId, id))
            return Forbid();

        var project = await _db.Projects.FindAsync(id);
        if (project is null) return NotFound();

        var sw = Stopwatch.StartNew();

        // Recursive CTE: walks Project→Epics→Stories→Tasks→Subtasks in one SQL query.
        // Each iteration produces one level: Epics(round 1) → Stories(round 2) → Tasks(round 3) → Subtasks(round 4).
        var nodes = await _db.TreeNodes.FromSqlInterpolated($"""
            WITH NodeCTE AS (
                SELECT
                    CAST('Epic' AS NVARCHAR(10)) AS NodeType,
                    e.Id AS NodeId,
                    e.Id AS EpicId,
                    CAST(NULL AS INT) AS StoryId,
                    CAST(NULL AS INT) AS TaskId,
                    e.Title AS Title,
                    e.Priority AS Priority,
                    ws.Name AS StatusName,
                    ISNULL(ws.Color, '') AS StatusColor,
                    1 AS Depth,
                    CAST(RIGHT('0000000000' + CAST(e.Id AS VARCHAR(10)), 10) AS NVARCHAR(200)) AS TreePath,
                    CAST(NULL AS INT) AS StoryPoints,
                    CAST(NULL AS DECIMAL(18,2)) AS EstimatedHours,
                    CAST(NULL AS DECIMAL(18,2)) AS LoggedHours
                FROM Epics e
                INNER JOIN WorkflowStatuses ws ON e.StatusId = ws.Id
                WHERE e.ProjectId = {id}

                UNION ALL

                SELECT
                    CAST('Story' AS NVARCHAR(10)),
                    s.Id, n.EpicId, s.Id, CAST(NULL AS INT),
                    s.Title, s.Priority, ws.Name, ISNULL(ws.Color, ''),
                    2,
                    CAST(n.TreePath + '/' + RIGHT('0000000000' + CAST(s.Id AS VARCHAR(10)), 10) AS NVARCHAR(200)),
                    s.StoryPoints, CAST(NULL AS DECIMAL(18,2)), CAST(NULL AS DECIMAL(18,2))
                FROM Stories s
                INNER JOIN WorkflowStatuses ws ON s.StatusId = ws.Id
                INNER JOIN NodeCTE n ON n.NodeType = 'Epic' AND n.NodeId = s.EpicId

                UNION ALL

                SELECT
                    CAST('Task' AS NVARCHAR(10)),
                    t.Id, n.EpicId, n.StoryId, t.Id,
                    t.Title, t.Priority, ws.Name, ISNULL(ws.Color, ''),
                    3,
                    CAST(n.TreePath + '/' + RIGHT('0000000000' + CAST(t.Id AS VARCHAR(10)), 10) AS NVARCHAR(200)),
                    CAST(NULL AS INT), t.EstimatedHours, t.LoggedHours
                FROM Tasks t
                INNER JOIN WorkflowStatuses ws ON t.StatusId = ws.Id
                INNER JOIN NodeCTE n ON n.NodeType = 'Story' AND n.NodeId = t.StoryId

                UNION ALL

                SELECT
                    CAST('Subtask' AS NVARCHAR(10)),
                    st.Id, n.EpicId, n.StoryId, n.TaskId,
                    st.Title, st.Priority, ws.Name, ISNULL(ws.Color, ''),
                    4,
                    CAST(n.TreePath + '/' + RIGHT('0000000000' + CAST(st.Id AS VARCHAR(10)), 10) AS NVARCHAR(200)),
                    CAST(NULL AS INT), CAST(NULL AS DECIMAL(18,2)), CAST(NULL AS DECIMAL(18,2))
                FROM Subtasks st
                INNER JOIN WorkflowStatuses ws ON st.StatusId = ws.Id
                INNER JOIN NodeCTE n ON n.NodeType = 'Task' AND n.NodeId = st.TaskId
            )
            SELECT NodeType, NodeId, EpicId, StoryId, TaskId, Title, Priority,
                   StatusName, StatusColor, Depth, TreePath, StoryPoints, EstimatedHours, LoggedHours
            FROM NodeCTE
            ORDER BY TreePath
            """).ToListAsync();

        sw.Stop();

        var epicNodes    = nodes.Where(n => n.NodeType == "Epic").ToList();
        var storyNodes   = nodes.Where(n => n.NodeType == "Story").ToList();
        var taskNodes    = nodes.Where(n => n.NodeType == "Task").ToList();
        var subtaskNodes = nodes.Where(n => n.NodeType == "Subtask").ToList();

        var tree = new ProjectTreeDto
        {
            ProjectId   = project.Id,
            ProjectName = project.Name,
            ProjectKey  = project.Key,
            EpicCount    = epicNodes.Count,
            StoryCount   = storyNodes.Count,
            TaskCount    = taskNodes.Count,
            SubtaskCount = subtaskNodes.Count,
            TotalNodes   = nodes.Count,
            QueryMs      = sw.ElapsedMilliseconds,
            Epics = epicNodes.Select(e => new EpicTreeNode
            {
                Id = e.NodeId, Title = e.Title, Priority = e.Priority,
                StatusName = e.StatusName, StatusColor = e.StatusColor,
                Stories = storyNodes.Where(s => s.EpicId == e.NodeId).Select(s => new StoryTreeNode
                {
                    Id = s.NodeId, Title = s.Title, Priority = s.Priority,
                    StatusName = s.StatusName, StatusColor = s.StatusColor,
                    StoryPoints = s.StoryPoints,
                    Tasks = taskNodes.Where(t => t.StoryId == s.NodeId).Select(t => new TaskTreeNode
                    {
                        Id = t.NodeId, Title = t.Title, Priority = t.Priority,
                        StatusName = t.StatusName, StatusColor = t.StatusColor,
                        EstimatedHours = t.EstimatedHours, LoggedHours = t.LoggedHours,
                        Subtasks = subtaskNodes.Where(st => st.TaskId == t.NodeId).Select(st => new SubtaskTreeNode
                        {
                            Id = st.NodeId, Title = st.Title, Priority = st.Priority,
                            StatusName = st.StatusName, StatusColor = st.StatusColor
                        }).ToList()
                    }).ToList()
                }).ToList()
            }).ToList()
        };

        return Ok(tree);
    }
}
