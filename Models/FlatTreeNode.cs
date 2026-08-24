namespace jira_lite.Models;

public class FlatTreeNode
{
    public string NodeType { get; set; } = string.Empty;
    public int NodeId { get; set; }
    public int? EpicId { get; set; }
    public int? StoryId { get; set; }
    public int? TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public int Depth { get; set; }
    public string TreePath { get; set; } = string.Empty;
    public int? StoryPoints { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? LoggedHours { get; set; }
}
