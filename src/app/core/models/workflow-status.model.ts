export interface WorkflowStatusDto {
  id: number;
  name: string;
  color: string;
  order: number;
  projectId?: number;
  isGlobal: boolean;
}

export interface CreateWorkflowStatusRequest {
  name: string;
  color: string;
  order: number;
}

export interface UpdateWorkflowStatusRequest {
  name: string;
  color: string;
  order: number;
}
