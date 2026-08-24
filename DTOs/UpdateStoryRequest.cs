using System.ComponentModel.DataAnnotations;

namespace jira_lite.DTOs;

public class UpdateStoryRequest
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [RegularExpression("Low|Medium|High|Critical")]
    public string Priority { get; set; } = "Medium";

    [Range(1, 100)]
    public int? StoryPoints { get; set; }

    public int? AssigneeId { get; set; }
    public int StatusId { get; set; }
}
