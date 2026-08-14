namespace jira_lite.Models;

public class Story
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public int? StoryPoints { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int EpicId { get; set; }
    public Epic Epic { get; set; } = null!;

    public int StatusId { get; set; }
    public WorkflowStatus Status { get; set; } = null!;

    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public int? AssigneeId { get; set; }
    public User? Assignee { get; set; }

    public ICollection<Task> Tasks { get; set; } = new List<Task>();
}
