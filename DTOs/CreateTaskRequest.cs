using System.ComponentModel.DataAnnotations;

namespace jira_lite.DTOs;

public class CreateTaskRequest
{
    public int StoryId { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [RegularExpression("Low|Medium|High|Critical")]
    public string Priority { get; set; } = "Medium";

    [Range(0.5, 999)]
    public decimal? EstimatedHours { get; set; }

    public int? AssigneeId { get; set; }
}
