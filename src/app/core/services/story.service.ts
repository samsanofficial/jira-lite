import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Story, CreateStoryRequest, UpdateStoryRequest } from '../models/story.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class StoryService {
  private readonly url = `${environment.apiUrl}/stories`;

  constructor(private http: HttpClient) {}

  getByEpic(epicId: number) {
    return this.http.get<Story[]>(`${this.url}?epicId=${epicId}`);
  }

  getById(id: number) {
    return this.http.get<Story>(`${this.url}/${id}`);
  }

  create(req: CreateStoryRequest) {
    return this.http.post<{ id: number }>(this.url, req);
  }

  update(id: number, req: UpdateStoryRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
