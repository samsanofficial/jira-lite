export interface Epic {
  id: number;
  title: string;
  description?: string;
  priority: string;
  dueDate?: string;
  createdAt: string;
  updatedAt: string;
  projectId: number;
  statusId: number;
  statusName: string;
  statusColor: string;
  createdById: number;
  createdByName: string;
  assigneeId?: number;
  assigneeName?: string;
  storyCount: number;
}

export interface CreateEpicRequest {
  projectId: number;
  title: string;
  description?: string;
  priority: string;
  dueDate?: string;
  assigneeId?: number;
}

export interface UpdateEpicRequest {
  title: string;
  description?: string;
  priority: string;
  dueDate?: string;
  assigneeId?: number;
  statusId: number;
}
