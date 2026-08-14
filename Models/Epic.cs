namespace jira_lite.Models;

public class Epic
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int StatusId { get; set; }
    public WorkflowStatus Status { get; set; } = null!;

    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public int? AssigneeId { get; set; }
    public User? Assignee { get; set; }

    public ICollection<Story> Stories { get; set; } = new List<Story>();
}
