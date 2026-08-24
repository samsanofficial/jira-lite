import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EpicService } from '../../../core/services/epic.service';
import { StoryService } from '../../../core/services/story.service';
import { AuthService } from '../../../core/services/auth.service';
import { Epic } from '../../../core/models/epic.model';
import { Story } from '../../../core/models/story.model';

@Component({
  selector: 'app-epic-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './epic-detail.component.html',
  styleUrl: './epic-detail.component.scss'
})
export class EpicDetailComponent implements OnInit {
  epic?: Epic;
  stories: Story[] = [];
  loading = true;
  error = '';
  deleteError = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private epicService: EpicService,
    private storyService: StoryService,
    public authService: AuthService
  ) {}

  get epicId(): number { return +this.route.snapshot.paramMap.get('id')!; }

  ngOnInit(): void { this.loadEpic(); }

  loadEpic(): void {
    this.loading = true;
    this.epicService.getById(this.epicId).subscribe({
      next: e => { this.epic = e; this.loadStories(); },
      error: () => { this.error = 'Failed to load epic.'; this.loading = false; }
    });
  }

  loadStories(): void {
    this.storyService.getByEpic(this.epicId).subscribe({
      next: data => { this.stories = data; this.loading = false; },
      error: () => { this.error = 'Failed to load stories.'; this.loading = false; }
    });
  }

  deleteStory(story: Story): void {
    if (!confirm(`Delete story "${story.title}"?`)) return;
    this.deleteError = '';
    this.storyService.delete(story.id).subscribe({
      next: () => this.loadStories(),
      error: err => {
        this.deleteError = err.status === 403 ? 'Lead or Admin role required to delete stories.' : 'Delete failed.';
      }
    });
  }

  priorityClass(priority: string): string { return priority.toLowerCase(); }
  logout(): void { this.authService.logout(); }
}
