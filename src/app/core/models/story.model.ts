export interface Story {
  id: number;
  title: string;
  description?: string;
  priority: string;
  storyPoints?: number;
  createdAt: string;
  updatedAt: string;
  epicId: number;
  epicTitle: string;
  projectId: number;
  statusId: number;
  statusName: string;
  statusColor: string;
  createdById: number;
  createdByName: string;
  assigneeId?: number;
  assigneeName?: string;
  taskCount: number;
}

export interface CreateStoryRequest {
  epicId: number;
  title: string;
  description?: string;
  priority: string;
  storyPoints?: number;
  assigneeId?: number;
}

export interface UpdateStoryRequest {
  title: string;
  description?: string;
  priority: string;
  storyPoints?: number;
  assigneeId?: number;
  statusId: number;
}
