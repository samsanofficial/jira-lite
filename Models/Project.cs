namespace jira_lite.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public ICollection<Epic> Epics { get; set; } = new List<Epic>();
    public ICollection<UserProjectRole> UserProjectRoles { get; set; } = new List<UserProjectRole>();
}
