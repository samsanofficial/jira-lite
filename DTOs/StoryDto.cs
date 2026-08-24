namespace jira_lite.DTOs;

public class StoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = string.Empty;
    public int? StoryPoints { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int EpicId { get; set; }
    public string EpicTitle { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public int? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public int TaskCount { get; set; }
}
