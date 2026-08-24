export interface SubtaskTreeNode {
  id: number;
  title: string;
  priority: string;
  statusName: string;
  statusColor: string;
}

export interface TaskTreeNode {
  id: number;
  title: string;
  priority: string;
  statusName: string;
  statusColor: string;
  estimatedHours?: number;
  loggedHours?: number;
  subtasks: SubtaskTreeNode[];
}

export interface StoryTreeNode {
  id: number;
  title: string;
  priority: string;
  statusName: string;
  statusColor: string;
  storyPoints?: number;
  tasks: TaskTreeNode[];
}

export interface EpicTreeNode {
  id: number;
  title: string;
  priority: string;
  statusName: string;
  statusColor: string;
  stories: StoryTreeNode[];
}

export interface ProjectTree {
  projectId: number;
  projectName: string;
  projectKey: string;
  epicCount: number;
  storyCount: number;
  taskCount: number;
  subtaskCount: number;
  totalNodes: number;
  queryMs: number;
  epics: EpicTreeNode[];
}
