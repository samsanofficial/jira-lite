using System.ComponentModel.DataAnnotations;

namespace jira_lite.DTOs;

public class WorkflowStatusDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Order { get; set; }
    public int? ProjectId { get; set; }
    public bool IsGlobal { get; set; }
}

public class CreateWorkflowStatusRequest
{
    [Required][MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(20)] public string Color { get; set; } = "#cccccc";
    public int Order { get; set; }
}

public class UpdateWorkflowStatusRequest
{
    [Required][MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(20)] public string Color { get; set; } = "#cccccc";
    public int Order { get; set; }
}
