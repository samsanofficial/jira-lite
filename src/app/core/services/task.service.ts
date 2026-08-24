import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Task, CreateTaskRequest, UpdateTaskRequest } from '../models/task.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly url = `${environment.apiUrl}/tasks`;

  constructor(private http: HttpClient) {}

  getByStory(storyId: number) {
    return this.http.get<Task[]>(`${this.url}?storyId=${storyId}`);
  }

  getById(id: number) {
    return this.http.get<Task>(`${this.url}/${id}`);
  }

  create(req: CreateTaskRequest) {
    return this.http.post<{ id: number }>(this.url, req);
  }

  update(id: number, req: UpdateTaskRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
