namespace jira_lite.Models;

public class WorkflowStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#cccccc";
    public int Order { get; set; }

    public ICollection<WorkflowTransition> FromTransitions { get; set; } = new List<WorkflowTransition>();
    public ICollection<WorkflowTransition> ToTransitions { get; set; } = new List<WorkflowTransition>();
}
