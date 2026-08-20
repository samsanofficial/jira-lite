export interface Project {
  id: number;
  name: string;
  key: string;
  description?: string;
  startDate?: string;
  endDate?: string;
  createdAt: string;
  updatedAt: string;
  createdById: number;
  createdByName: string;
}

export interface CreateProjectRequest {
  name: string;
  key: string;
  description?: string;
  startDate?: string;
  endDate?: string;
}

export interface UpdateProjectRequest {
  name: string;
  description?: string;
  startDate?: string;
  endDate?: string;
}
