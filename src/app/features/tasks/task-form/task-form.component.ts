import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TaskService } from '../../../core/services/task.service';

const PRIORITIES = ['Low', 'Medium', 'High', 'Critical'];
const STATUSES = [
  { id: 1, name: 'Todo' }, { id: 2, name: 'In Progress' },
  { id: 3, name: 'In Review' }, { id: 4, name: 'Done' }
];

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.scss'
})
export class TaskFormComponent implements OnInit {
  form: FormGroup;
  isEdit = false;
  taskId?: number;
  storyId?: number;
  loading = false;
  loadError = '';
  saveError = '';
  priorities = PRIORITIES;
  statuses = STATUSES;

  constructor(
    private fb: FormBuilder,
    private taskService: TaskService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      title:          ['', [Validators.required, Validators.maxLength(300)]],
      description:    ['', Validators.maxLength(2000)],
      priority:       ['Medium', Validators.required],
      estimatedHours: [null, [Validators.min(0.5), Validators.max(999)]],
      loggedHours:    [null, [Validators.min(0), Validators.max(999)]],
      statusId:       [1]
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.taskId = +id;
      this.form.get('loggedHours')!.enable();
      this.loadTask(this.taskId);
    } else {
      this.storyId = +this.route.snapshot.paramMap.get('storyId')!;
      this.form.get('loggedHours')!.disable();
    }
  }

  private loadTask(id: number): void {
    this.loading = true;
    this.taskService.getById(id).subscribe({
      next: t => {
        this.storyId = t.storyId;
        this.form.patchValue({
          title:          t.title,
          description:    t.description ?? '',
          priority:       t.priority,
          estimatedHours: t.estimatedHours ?? null,
          loggedHours:    t.loggedHours ?? null,
          statusId:       t.statusId
        });
        this.loading = false;
      },
      error: () => { this.loadError = 'Failed to load task.'; this.loading = false; }
    });
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    this.saveError = '';
    const v = this.form.getRawValue();

    const base = {
      title:          v.title,
      description:    v.description || undefined,
      priority:       v.priority,
      estimatedHours: v.estimatedHours || undefined
    };

    if (this.isEdit) {
      this.taskService.update(this.taskId!, { ...base, loggedHours: v.loggedHours || undefined, statusId: v.statusId }).subscribe({
        next: () => this.router.navigate(['/stories', this.storyId]),
        error: err => { this.saveError = err.status === 403 ? 'Member or above role required.' : 'Save failed.'; this.loading = false; }
      });
    } else {
      this.taskService.create({ ...base, storyId: this.storyId! }).subscribe({
        next: () => this.router.navigate(['/stories', this.storyId]),
        error: err => { this.saveError = err.status === 403 ? 'Member or above role required.' : 'Create failed.'; this.loading = false; }
      });
    }
  }

  f(n: string) { return this.form.get(n); }
  invalid(n: string) { return this.f(n)?.invalid && this.f(n)?.touched; }
}
