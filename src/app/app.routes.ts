import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'projects', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'projects',
    canActivate: [authGuard],
    loadComponent: () => import('./features/projects/project-list/project-list.component').then(m => m.ProjectListComponent)
  },
  {
    path: 'projects/new',
    canActivate: [authGuard],
    loadComponent: () => import('./features/projects/project-form/project-form.component').then(m => m.ProjectFormComponent)
  },
  {
    path: 'projects/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/projects/project-detail/project-detail.component').then(m => m.ProjectDetailComponent)
  },
  {
    path: 'projects/:id/edit',
    canActivate: [authGuard],
    loadComponent: () => import('./features/projects/project-form/project-form.component').then(m => m.ProjectFormComponent)
  },
  {
    path: 'projects/:projectId/epics/new',
    canActivate: [authGuard],
    loadComponent: () => import('./features/epics/epic-form/epic-form.component').then(m => m.EpicFormComponent)
  },
  {
    path: 'epics/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/epics/epic-detail/epic-detail.component').then(m => m.EpicDetailComponent)
  },
  {
    path: 'epics/:id/edit',
    canActivate: [authGuard],
    loadComponent: () => import('./features/epics/epic-form/epic-form.component').then(m => m.EpicFormComponent)
  },
  {
    path: 'epics/:epicId/stories/new',
    canActivate: [authGuard],
    loadComponent: () => import('./features/stories/story-form/story-form.component').then(m => m.StoryFormComponent)
  },
  {
    path: 'stories/:id/edit',
    canActivate: [authGuard],
    loadComponent: () => import('./features/stories/story-form/story-form.component').then(m => m.StoryFormComponent)
  },
  { path: '**', redirectTo: 'projects' }
];
