using System.ComponentModel.DataAnnotations;

namespace jira_lite.DTOs;

public class CreateSubtaskRequest
{
    public int TaskId { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [RegularExpression("Low|Medium|High|Critical")]
    public string Priority { get; set; } = "Medium";

    public int? AssigneeId { get; set; }
}
