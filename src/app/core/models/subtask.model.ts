export interface Subtask {
  id: number;
  title: string;
  description?: string;
  priority: string;
  createdAt: string;
  updatedAt: string;
  taskId: number;
  taskTitle: string;
  statusId: number;
  statusName: string;
  statusColor: string;
  createdById: number;
  createdByName: string;
  assigneeId?: number;
  assigneeName?: string;
}

export interface CreateSubtaskRequest {
  taskId: number;
  title: string;
  description?: string;
  priority: string;
  assigneeId?: number;
}

export interface UpdateSubtaskRequest {
  title: string;
  description?: string;
  priority: string;
  assigneeId?: number;
  statusId: number;
}
