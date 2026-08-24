namespace jira_lite.DTOs;

public class ProjectTreeDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
    public int EpicCount { get; set; }
    public int StoryCount { get; set; }
    public int TaskCount { get; set; }
    public int SubtaskCount { get; set; }
    public int TotalNodes { get; set; }
    public long QueryMs { get; set; }
    public List<EpicTreeNode> Epics { get; set; } = [];
}

public class EpicTreeNode
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public List<StoryTreeNode> Stories { get; set; } = [];
}

public class StoryTreeNode
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public int? StoryPoints { get; set; }
    public List<TaskTreeNode> Tasks { get; set; } = [];
}

public class TaskTreeNode
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public decimal? EstimatedHours { get; set; }
    public decimal? LoggedHours { get; set; }
    public List<SubtaskTreeNode> Subtasks { get; set; } = [];
}

public class SubtaskTreeNode
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
}
