using jira_lite.Data;
using Microsoft.EntityFrameworkCore;

namespace jira_lite.Services;

public class ProjectRoleService
{
    private readonly AppDbContext _db;

    public ProjectRoleService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string?> GetUserRoleAsync(int userId, int projectId)
    {
        var entry = await _db.UserProjectRoles
            .Include(upr => upr.Role)
            .FirstOrDefaultAsync(upr => upr.UserId == userId && upr.ProjectId == projectId);

        return entry?.Role.Name;
    }

    public async Task<bool> IsAdminAsync(int userId, int projectId)
        => await GetUserRoleAsync(userId, projectId) == "Project Admin";

    public async Task<bool> IsLeadOrAboveAsync(int userId, int projectId)
    {
        var role = await GetUserRoleAsync(userId, projectId);
        return role is "Project Admin" or "Lead";
    }

    public async Task<bool> IsMemberOrAboveAsync(int userId, int projectId)
    {
        var role = await GetUserRoleAsync(userId, projectId);
        return role is "Project Admin" or "Lead" or "Member";
    }

    public async Task<bool> HasAnyAccessAsync(int userId, int projectId)
        => await GetUserRoleAsync(userId, projectId) is not null;
}
