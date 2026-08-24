import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ProjectTree } from '../models/tree.model';

@Injectable({ providedIn: 'root' })
export class TreeService {
  private base = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getProjectTree(projectId: number): Observable<ProjectTree> {
    return this.http.get<ProjectTree>(`${this.base}/projects/${projectId}/tree`);
  }
}
