import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Project, CreateProjectRequest, UpdateProjectRequest } from '../models/project.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private readonly url = `${environment.apiUrl}/projects`;

  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<Project[]>(this.url);
  }

  getById(id: number) {
    return this.http.get<Project>(`${this.url}/${id}`);
  }

  create(req: CreateProjectRequest) {
    return this.http.post<Project>(this.url, req);
  }

  update(id: number, req: UpdateProjectRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
