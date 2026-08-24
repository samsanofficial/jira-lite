import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProjectService } from '../../../core/services/project.service';
import { EpicService } from '../../../core/services/epic.service';
import { AuthService } from '../../../core/services/auth.service';
import { Project } from '../../../core/models/project.model';
import { Epic } from '../../../core/models/epic.model';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './project-detail.component.html',
  styleUrl: './project-detail.component.scss'
})
export class ProjectDetailComponent implements OnInit {
  project?: Project;
  epics: Epic[] = [];
  loading = true;
  error = '';
  deleteError = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private projectService: ProjectService,
    private epicService: EpicService,
    public authService: AuthService
  ) {}

  get projectId(): number {
    return +this.route.snapshot.paramMap.get('id')!;
  }

  ngOnInit(): void {
    this.loadProject();
  }

  loadProject(): void {
    this.loading = true;
    this.projectService.getById(this.projectId).subscribe({
      next: p => {
        this.project = p;
        this.loadEpics();
      },
      error: () => { this.error = 'Failed to load project.'; this.loading = false; }
    });
  }

  loadEpics(): void {
    this.epicService.getByProject(this.projectId).subscribe({
      next: data => { this.epics = data; this.loading = false; },
      error: () => { this.error = 'Failed to load epics.'; this.loading = false; }
    });
  }

  deleteEpic(epic: Epic): void {
    if (!confirm(`Delete epic "${epic.title}"? All stories inside will also be deleted.`)) return;
    this.deleteError = '';
    this.epicService.delete(epic.id).subscribe({
      next: () => this.loadEpics(),
      error: err => {
        this.deleteError = err.status === 403 ? 'Only a Project Admin can delete epics.' : 'Delete failed.';
      }
    });
  }

  priorityClass(priority: string): string {
    return priority.toLowerCase();
  }

  logout(): void { this.authService.logout(); }
}
