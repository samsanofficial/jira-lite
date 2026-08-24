import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { WorkflowStatusDto, CreateWorkflowStatusRequest, UpdateWorkflowStatusRequest } from '../models/workflow-status.model';

@Injectable({ providedIn: 'root' })
export class WorkflowStatusService {
  private base = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getByProject(projectId: number): Observable<WorkflowStatusDto[]> {
    return this.http.get<WorkflowStatusDto[]>(`${this.base}/projects/${projectId}/statuses`);
  }

  create(projectId: number, body: CreateWorkflowStatusRequest): Observable<WorkflowStatusDto> {
    return this.http.post<WorkflowStatusDto>(`${this.base}/projects/${projectId}/statuses`, body);
  }

  update(projectId: number, id: number, body: UpdateWorkflowStatusRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/projects/${projectId}/statuses/${id}`, body);
  }

  delete(projectId: number, id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/projects/${projectId}/statuses/${id}`);
  }
}
