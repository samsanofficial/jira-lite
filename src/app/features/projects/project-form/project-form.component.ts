import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProjectService } from '../../../core/services/project.service';

@Component({
  selector: 'app-project-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './project-form.component.html',
  styleUrl: './project-form.component.scss'
})
export class ProjectFormComponent implements OnInit {
  form: FormGroup;
  isEdit = false;
  projectId?: number;
  loading = false;
  loadError = '';
  saveError = '';

  constructor(
    private fb: FormBuilder,
    private projectService: ProjectService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      name:        ['', [Validators.required, Validators.maxLength(200)]],
      key:         ['', [Validators.required, Validators.maxLength(10), Validators.pattern(/^[A-Za-z0-9]+$/)]],
      description: ['', Validators.maxLength(1000)],
      startDate:   [''],
      endDate:     ['']
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.projectId = +id;
      this.form.get('key')!.disable();
      this.loadProject(this.projectId);
    }
  }

  private loadProject(id: number): void {
    this.loading = true;
    this.projectService.getById(id).subscribe({
      next: p => {
        this.form.patchValue({
          name:        p.name,
          key:         p.key,
          description: p.description ?? '',
          startDate:   p.startDate ? p.startDate.substring(0, 10) : '',
          endDate:     p.endDate   ? p.endDate.substring(0, 10)   : ''
        });
        this.loading = false;
      },
      error: () => { this.loadError = 'Failed to load project.'; this.loading = false; }
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.saveError = '';

    const raw = this.form.getRawValue();
    const payload = {
      name:        raw.name,
      description: raw.description || undefined,
      startDate:   raw.startDate   || undefined,
      endDate:     raw.endDate     || undefined
    };

    if (this.isEdit) {
      this.projectService.update(this.projectId!, payload).subscribe({
        next: () => this.router.navigate(['/projects']),
        error: err => {
          this.saveError = err.status === 403 ? 'You need Lead or Admin role to edit this project.' : 'Save failed.';
          this.loading = false;
        }
      });
    } else {
      this.projectService.create({ ...payload, key: raw.key }).subscribe({
        next: () => this.router.navigate(['/projects']),
        error: err => {
          this.saveError = err.status === 409
            ? `Project key "${raw.key.toUpperCase()}" is already in use.`
            : 'Create failed. Please try again.';
          this.loading = false;
        }
      });
    }
  }

  f(name: string) { return this.form.get(name); }
  invalid(name: string) { return this.f(name)?.invalid && this.f(name)?.touched; }
}
