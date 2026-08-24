import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TaskService } from '../../../core/services/task.service';
import { SubtaskService } from '../../../core/services/subtask.service';
import { AuthService } from '../../../core/services/auth.service';
import { Task } from '../../../core/models/task.model';
import { Subtask } from '../../../core/models/subtask.model';

@Component({
  selector: 'app-task-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './task-detail.component.html',
  styleUrl: './task-detail.component.scss'
})
export class TaskDetailComponent implements OnInit {
  task?: Task;
  subtasks: Subtask[] = [];
  loading = true;
  error = '';
  deleteError = '';

  constructor(
    private route: ActivatedRoute,
    private taskService: TaskService,
    private subtaskService: SubtaskService,
    public authService: AuthService
  ) {}

  get taskId(): number { return +this.route.snapshot.paramMap.get('id')!; }

  ngOnInit(): void { this.loadTask(); }

  loadTask(): void {
    this.loading = true;
    this.taskService.getById(this.taskId).subscribe({
      next: t => { this.task = t; this.loadSubtasks(); },
      error: () => { this.error = 'Failed to load task.'; this.loading = false; }
    });
  }

  loadSubtasks(): void {
    this.subtaskService.getByTask(this.taskId).subscribe({
      next: data => { this.subtasks = data; this.loading = false; },
      error: () => { this.error = 'Failed to load subtasks.'; this.loading = false; }
    });
  }

  deleteSubtask(sub: Subtask): void {
    if (!confirm(`Delete subtask "${sub.title}"?`)) return;
    this.deleteError = '';
    this.subtaskService.delete(sub.id).subscribe({
      next: () => this.loadSubtasks(),
      error: err => {
        this.deleteError = err.status === 403 ? 'Lead or Admin role required to delete subtasks.' : 'Delete failed.';
      }
    });
  }

  priorityClass(p: string): string { return p.toLowerCase(); }
  logout(): void { this.authService.logout(); }
}
