namespace jira_lite.Models;

public class Issue
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Todo";
    public string Priority { get; set; } = "Medium";
    public string Type { get; set; } = "Task";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}
