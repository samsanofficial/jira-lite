export interface Task {
  id: number;
  title: string;
  description?: string;
  priority: string;
  estimatedHours?: number;
  loggedHours?: number;
  createdAt: string;
  updatedAt: string;
  storyId: number;
  storyTitle: string;
  epicId: number;
  projectId: number;
  statusId: number;
  statusName: string;
  statusColor: string;
  createdById: number;
  createdByName: string;
  assigneeId?: number;
  assigneeName?: string;
  subtaskCount: number;
}

export interface CreateTaskRequest {
  storyId: number;
  title: string;
  description?: string;
  priority: string;
  estimatedHours?: number;
  assigneeId?: number;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  priority: string;
  estimatedHours?: number;
  loggedHours?: number;
  assigneeId?: number;
  statusId: number;
}
