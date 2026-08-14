namespace jira_lite.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<UserProjectRole> UserProjectRoles { get; set; } = new List<UserProjectRole>();
}
