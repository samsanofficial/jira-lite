import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { TreeService } from '../../../core/services/tree.service';
import { ProjectTree } from '../../../core/models/tree.model';

@Component({
  selector: 'app-project-tree',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './project-tree.component.html',
  styleUrls: ['./project-tree.component.scss']
})
export class ProjectTreeComponent implements OnInit {
  projectId!: number;
  tree: ProjectTree | null = null;
  loading = true;
  error = '';
  expanded = new Set<string>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    public authService: AuthService,
    private treeService: TreeService
  ) {}

  ngOnInit(): void {
    this.projectId = +this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.treeService.getProjectTree(this.projectId).subscribe({
      next: tree => { this.tree = tree; this.loading = false; },
      error: err => { this.error = err.error?.message || 'Failed to load tree.'; this.loading = false; }
    });
  }

  toggle(key: string): void {
    this.expanded.has(key) ? this.expanded.delete(key) : this.expanded.add(key);
  }

  isExpanded(key: string): boolean {
    return this.expanded.has(key);
  }

  expandAll(): void {
    if (!this.tree) return;
    for (const epic of this.tree.epics) {
      this.expanded.add(`epic-${epic.id}`);
      for (const story of epic.stories) {
        this.expanded.add(`story-${story.id}`);
        for (const task of story.tasks) {
          this.expanded.add(`task-${task.id}`);
        }
      }
    }
  }

  collapseAll(): void {
    this.expanded.clear();
  }

  priorityClass(p: string): string {
    return p?.toLowerCase() || 'medium';
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
