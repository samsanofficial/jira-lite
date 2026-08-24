import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Epic, CreateEpicRequest, UpdateEpicRequest } from '../models/epic.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class EpicService {
  private readonly url = `${environment.apiUrl}/epics`;

  constructor(private http: HttpClient) {}

  getByProject(projectId: number) {
    return this.http.get<Epic[]>(`${this.url}?projectId=${projectId}`);
  }

  getById(id: number) {
    return this.http.get<Epic>(`${this.url}/${id}`);
  }

  create(req: CreateEpicRequest) {
    return this.http.post<{ id: number }>(this.url, req);
  }

  update(id: number, req: UpdateEpicRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
