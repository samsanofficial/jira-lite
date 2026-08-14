namespace jira_lite.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserProjectRole> UserProjectRoles { get; set; } = new List<UserProjectRole>();
    public ICollection<Project> CreatedProjects { get; set; } = new List<Project>();
}
