import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EpicService } from '../../../core/services/epic.service';

const PRIORITIES = ['Low', 'Medium', 'High', 'Critical'];
const STATUSES = [
  { id: 1, name: 'Todo' }, { id: 2, name: 'In Progress' },
  { id: 3, name: 'In Review' }, { id: 4, name: 'Done' }
];

@Component({
  selector: 'app-epic-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './epic-form.component.html',
  styleUrl: './epic-form.component.scss'
})
export class EpicFormComponent implements OnInit {
  form: FormGroup;
  isEdit = false;
  epicId?: number;
  projectId?: number;
  loading = false;
  loadError = '';
  saveError = '';
  priorities = PRIORITIES;
  statuses = STATUSES;

  constructor(
    private fb: FormBuilder,
    private epicService: EpicService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      title:       ['', [Validators.required, Validators.maxLength(300)]],
      description: ['', Validators.maxLength(2000)],
      priority:    ['Medium', Validators.required],
      dueDate:     [''],
      statusId:    [1]
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.epicId = +id;
      this.loadEpic(this.epicId);
    } else {
      this.projectId = +this.route.snapshot.paramMap.get('projectId')!;
    }
  }

  private loadEpic(id: number): void {
    this.loading = true;
    this.epicService.getById(id).subscribe({
      next: e => {
        this.projectId = e.projectId;
        this.form.patchValue({
          title:       e.title,
          description: e.description ?? '',
          priority:    e.priority,
          dueDate:     e.dueDate ? e.dueDate.substring(0, 10) : '',
          statusId:    e.statusId
        });
        this.loading = false;
      },
      error: () => { this.loadError = 'Failed to load epic.'; this.loading = false; }
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
      dueDate:     v.dueDate || undefined
    };

    if (this.isEdit) {
      this.epicService.update(this.epicId!, { ...base, statusId: v.statusId }).subscribe({
        next: () => this.router.navigate(['/projects', this.projectId]),
        error: err => { this.saveError = err.status === 403 ? 'Lead or Admin role required.' : 'Save failed.'; this.loading = false; }
      });
    } else {
      this.epicService.create({ ...base, projectId: this.projectId! }).subscribe({
        next: () => this.router.navigate(['/projects', this.projectId]),
        error: err => { this.saveError = err.status === 403 ? 'Lead or Admin role required.' : 'Create failed.'; this.loading = false; }
      });
    }
  }

  f(n: string) { return this.form.get(n); }
  invalid(n: string) { return this.f(n)?.invalid && this.f(n)?.touched; }
}
