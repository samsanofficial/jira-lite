import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { StoryService } from '../../../core/services/story.service';
import { TaskService } from '../../../core/services/task.service';
import { AuthService } from '../../../core/services/auth.service';
import { Story } from '../../../core/models/story.model';
import { Task } from '../../../core/models/task.model';

@Component({
  selector: 'app-story-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './story-detail.component.html',
  styleUrl: './story-detail.component.scss'
})
export class StoryDetailComponent implements OnInit {
  story?: Story;
  tasks: Task[] = [];
  loading = true;
  error = '';
  deleteError = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private storyService: StoryService,
    private taskService: TaskService,
    public authService: AuthService
  ) {}

  get storyId(): number { return +this.route.snapshot.paramMap.get('id')!; }

  ngOnInit(): void { this.loadStory(); }

  loadStory(): void {
    this.loading = true;
    this.storyService.getById(this.storyId).subscribe({
      next: s => { this.story = s; this.loadTasks(); },
      error: () => { this.error = 'Failed to load story.'; this.loading = false; }
    });
  }

  loadTasks(): void {
    this.taskService.getByStory(this.storyId).subscribe({
      next: data => { this.tasks = data; this.loading = false; },
      error: () => { this.error = 'Failed to load tasks.'; this.loading = false; }
    });
  }

  deleteTask(task: Task): void {
    if (!confirm(`Delete task "${task.title}"? All subtasks will also be deleted.`)) return;
    this.deleteError = '';
    this.taskService.delete(task.id).subscribe({
      next: () => this.loadTasks(),
      error: err => {
        this.deleteError = err.status === 403 ? 'Lead or Admin role required to delete tasks.' : 'Delete failed.';
      }
    });
  }

  priorityClass(p: string): string { return p.toLowerCase(); }
  logout(): void { this.authService.logout(); }
}
