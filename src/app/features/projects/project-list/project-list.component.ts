import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ProjectService } from '../../../core/services/project.service';
import { AuthService } from '../../../core/services/auth.service';
import { Project } from '../../../core/models/project.model';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './project-list.component.html',
  styleUrl: './project-list.component.scss'
})
export class ProjectListComponent implements OnInit {
  projects: Project[] = [];
  loading = true;
  error = '';
  deleteError = '';

  constructor(
    private projectService: ProjectService,
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.projectService.getAll().subscribe({
      next: data => { this.projects = data; this.loading = false; },
      error: () => { this.error = 'Failed to load projects.'; this.loading = false; }
    });
  }

  edit(id: number): void {
    this.router.navigate(['/projects', id, 'edit']);
  }

  delete(project: Project): void {
    if (!confirm(`Delete project "${project.name}"? This cannot be undone.`)) return;
    this.deleteError = '';
    this.projectService.delete(project.id).subscribe({
      next: () => this.load(),
      error: err => {
        this.deleteError = err.status === 403
          ? 'Only a Project Admin can delete this project.'
          : 'Delete failed. Please try again.';
      }
    });
  }

  logout(): void {
    this.authService.logout();
  }
}
