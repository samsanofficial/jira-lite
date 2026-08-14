namespace jira_lite.Models;

public class WorkflowTransition
{
    public int Id { get; set; }
    public int FromStatusId { get; set; }
    public int ToStatusId { get; set; }
    public string EntityType { get; set; } = string.Empty; // Epic, Story, Task, Subtask

    public WorkflowStatus FromStatus { get; set; } = null!;
    public WorkflowStatus ToStatus { get; set; } = null!;
}
