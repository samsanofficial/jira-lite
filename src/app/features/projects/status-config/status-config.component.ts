import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { WorkflowStatusService } from '../../../core/services/workflow-status.service';
import { WorkflowStatusDto } from '../../../core/models/workflow-status.model';

@Component({
  selector: 'app-status-config',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './status-config.component.html',
  styleUrls: ['./status-config.component.scss']
})
export class StatusConfigComponent implements OnInit {
  projectId!: number;
  statuses: WorkflowStatusDto[] = [];
  loading = true;
  error = '';
  saveError = '';
  deleteError = '';

  addForm!: FormGroup;
  addMode = false;
  addSaving = false;

  editId: number | null = null;
  editForm!: FormGroup;
  editSaving = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    public authService: AuthService,
    private statusService: WorkflowStatusService
  ) {}

  ngOnInit(): void {
    this.projectId = +this.route.snapshot.paramMap.get('id')!;
    this.addForm = this.fb.group({
      name:  ['', [Validators.required, Validators.maxLength(100)]],
      color: ['#6b778c'],
      order: [0]
    });
    this.editForm = this.fb.group({
      name:  ['', [Validators.required, Validators.maxLength(100)]],
      color: ['#6b778c'],
      order: [0]
    });
    this.load();
  }

  load(): void {
    this.loading = true;
    this.statusService.getByProject(this.projectId).subscribe({
      next: data => { this.statuses = data; this.loading = false; },
      error: () => { this.error = 'Failed to load statuses.'; this.loading = false; }
    });
  }

  startAdd(): void { this.addMode = true; this.addForm.reset({ name: '', color: '#6b778c', order: 0 }); }
  cancelAdd(): void { this.addMode = false; }

  submitAdd(): void {
    if (this.addForm.invalid) return;
    this.addSaving = true;
    this.saveError = '';
    const v = this.addForm.value;
    this.statusService.create(this.projectId, { name: v.name, color: v.color, order: v.order }).subscribe({
      next: () => { this.addMode = false; this.addSaving = false; this.load(); },
      error: err => { this.saveError = err.error?.message || 'Failed to create status.'; this.addSaving = false; }
    });
  }

  startEdit(s: WorkflowStatusDto): void {
    if (s.isGlobal) return;
    this.editId = s.id;
    this.editForm.setValue({ name: s.name, color: s.color, order: s.order });
    this.saveError = '';
  }

  cancelEdit(): void { this.editId = null; }

  submitEdit(): void {
    if (this.editForm.invalid || this.editId === null) return;
    this.editSaving = true;
    this.saveError = '';
    const v = this.editForm.value;
    this.statusService.update(this.projectId, this.editId, { name: v.name, color: v.color, order: v.order }).subscribe({
      next: () => { this.editId = null; this.editSaving = false; this.load(); },
      error: err => { this.saveError = err.error?.message || 'Failed to update status.'; this.editSaving = false; }
    });
  }

  deleteStatus(s: WorkflowStatusDto): void {
    if (s.isGlobal) return;
    if (!confirm(`Delete status "${s.name}"? This cannot be undone.`)) return;
    this.deleteError = '';
    this.statusService.delete(this.projectId, s.id).subscribe({
      next: () => this.load(),
      error: err => this.deleteError = err.error?.message || 'Failed to delete status.'
    });
  }

  invalid(form: FormGroup, field: string): boolean {
    const c = form.get(field);
    return !!(c && c.invalid && (c.dirty || c.touched));
  }

  logout(): void { this.authService.logout(); this.router.navigate(['/login']); }
}
