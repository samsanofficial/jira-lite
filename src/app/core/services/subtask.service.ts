import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subtask, CreateSubtaskRequest, UpdateSubtaskRequest } from '../models/subtask.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SubtaskService {
  private readonly url = `${environment.apiUrl}/subtasks`;

  constructor(private http: HttpClient) {}

  getByTask(taskId: number) {
    return this.http.get<Subtask[]>(`${this.url}?taskId=${taskId}`);
  }

  getById(id: number) {
    return this.http.get<Subtask>(`${this.url}/${id}`);
  }

  create(req: CreateSubtaskRequest) {
    return this.http.post<{ id: number }>(this.url, req);
  }

  update(id: number, req: UpdateSubtaskRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
