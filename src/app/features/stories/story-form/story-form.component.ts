import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { StoryService } from '../../../core/services/story.service';

const PRIORITIES = ['Low', 'Medium', 'High', 'Critical'];
const STATUSES = [
  { id: 1, name: 'Todo' }, { id: 2, name: 'In Progress' },
  { id: 3, name: 'In Review' }, { id: 4, name: 'Done' }
];

@Component({
  selector: 'app-story-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './story-form.component.html',
  styleUrl: './story-form.component.scss'
})
export class StoryFormComponent implements OnInit {
  form: FormGroup;
  isEdit = false;
  storyId?: number;
  epicId?: number;
  loading = false;
  loadError = '';
  saveError = '';
  priorities = PRIORITIES;
  statuses = STATUSES;

  constructor(
    private fb: FormBuilder,
    private storyService: StoryService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      title:       ['', [Validators.required, Validators.maxLength(300)]],
      description: ['', Validators.maxLength(2000)],
      priority:    ['Medium', Validators.required],
      storyPoints: [null, [Validators.min(1), Validators.max(100)]],
      statusId:    [1]
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.storyId = +id;
      this.loadStory(this.storyId);
    } else {
      this.epicId = +this.route.snapshot.paramMap.get('epicId')!;
    }
  }

  private loadStory(id: number): void {
    this.loading = true;
    this.storyService.getById(id).subscribe({
      next: s => {
        this.epicId = s.epicId;
        this.form.patchValue({
          title:       s.title,
          description: s.description ?? '',
          priority:    s.priority,
          storyPoints: s.storyPoints ?? null,
          statusId:    s.statusId
        });
        this.loading = false;
      },
      error: () => { this.loadError = 'Failed to load story.'; this.loading = false; }
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    this.saveError = '';
    const v = this.form.value;

    const base = {
      title:       v.title,
      description: v.description || undefined,
      priority:    v.priority,
      storyPoints: v.storyPoints || undefined
    };

    if (this.isEdit) {
      this.storyService.update(this.storyId!, { ...base, statusId: v.statusId }).subscribe({
        next: () => this.router.navigate(['/epics', this.epicId]),
        error: err => { this.saveError = err.status === 403 ? 'Member or above role required.' : 'Save failed.'; this.loading = false; }
      });
    } else {
      this.storyService.create({ ...base, epicId: this.epicId! }).subscribe({
        next: () => this.router.navigate(['/epics', this.epicId]),
        error: err => { this.saveError = err.status === 403 ? 'Member or above role required.' : 'Create failed.'; this.loading = false; }
      });
    }
  }

  f(n: string) { return this.form.get(n); }
  invalid(n: string) { return this.f(n)?.invalid && this.f(n)?.touched; }
}
