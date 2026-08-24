import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { SubtaskService } from '../../../core/services/subtask.service';

const PRIORITIES = ['Low', 'Medium', 'High', 'Critical'];
const STATUSES = [
  { id: 1, name: 'Todo' }, { id: 2, name: 'In Progress' },
  { id: 3, name: 'In Review' }, { id: 4, name: 'Done' }
];

@Component({
  selector: 'app-subtask-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './subtask-form.component.html',
  styleUrl: './subtask-form.component.scss'
})
export class SubtaskFormComponent implements OnInit {
  form: FormGroup;
  isEdit = false;
  subtaskId?: number;
  taskId?: number;
  loading = false;
  loadError = '';
  saveError = '';
  priorities = PRIORITIES;
  statuses = STATUSES;

  constructor(
    private fb: FormBuilder,
    private subtaskService: SubtaskService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      title:       ['', [Validators.required, Validators.maxLength(300)]],
      description: ['', Validators.maxLength(2000)],
      priority:    ['Medium', Validators.required],
      statusId:    [1]
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.subtaskId = +id;
      this.loadSubtask(this.subtaskId);
    } else {
      this.taskId = +this.route.snapshot.paramMap.get('taskId')!;
    }
  }

  private loadSubtask(id: number): void {
    this.loading = true;
    this.subtaskService.getById(id).subscribe({
      next: s => {
        this.taskId = s.taskId;
        this.form.patchValue({
          title:       s.title,
          description: s.description ?? '',
          priority:    s.priority,
          statusId:    s.statusId
        });
        this.loading = false;
      },
      error: () => { this.loadError = 'Failed to load subtask.'; this.loading = false; }
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
      priority:    v.priority
    };

    if (this.isEdit) {
      this.subtaskService.update(this.subtaskId!, { ...base, statusId: v.statusId }).subscribe({
        next: () => this.router.navigate(['/tasks', this.taskId]),
        error: err => { this.saveError = err.status === 403 ? 'Member or above role required.' : 'Save failed.'; this.loading = false; }
      });
    } else {
      this.subtaskService.create({ ...base, taskId: this.taskId! }).subscribe({
        next: () => this.router.navigate(['/tasks', this.taskId]),
        error: err => { this.saveError = err.status === 403 ? 'Member or above role required.' : 'Create failed.'; this.loading = false; }
      });
    }
  }

  f(n: string) { return this.form.get(n); }
  invalid(n: string) { return this.f(n)?.invalid && this.f(n)?.touched; }
}
