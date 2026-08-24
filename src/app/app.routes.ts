import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'projects', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  // Projects
  { path: 'projects', canActivate: [authGuard],
    loadComponent: () => import('./features/projects/project-list/project-list.component').then(m => m.ProjectListComponent) },
  { path: 'projects/new', canActivate: [authGuard],
    loadComponent: () => import('./features/projects/project-form/project-form.component').then(m => m.ProjectFormComponent) },
  { path: 'projects/:id', canActivate: [authGuard],
    loadComponent: () => import('./features/projects/project-detail/project-detail.component').then(m => m.ProjectDetailComponent) },
  { path: 'projects/:id/edit', canActivate: [authGuard],
    loadComponent: () => import('./features/projects/project-form/project-form.component').then(m => m.ProjectFormComponent) },
  { path: 'projects/:id/tree', canActivate: [authGuard],
    loadComponent: () => import('./features/projects/project-tree/project-tree.component').then(m => m.ProjectTreeComponent) },
  { path: 'projects/:id/statuses', canActivate: [authGuard],
    loadComponent: () => import('./features/projects/status-config/status-config.component').then(m => m.StatusConfigComponent) },
  // Epics
  { path: 'projects/:projectId/epics/new', canActivate: [authGuard],
    loadComponent: () => import('./features/epics/epic-form/epic-form.component').then(m => m.EpicFormComponent) },
  { path: 'epics/:id', canActivate: [authGuard],
    loadComponent: () => import('./features/epics/epic-detail/epic-detail.component').then(m => m.EpicDetailComponent) },
  { path: 'epics/:id/edit', canActivate: [authGuard],
    loadComponent: () => import('./features/epics/epic-form/epic-form.component').then(m => m.EpicFormComponent) },
  // Stories
  { path: 'epics/:epicId/stories/new', canActivate: [authGuard],
    loadComponent: () => import('./features/stories/story-form/story-form.component').then(m => m.StoryFormComponent) },
  { path: 'stories/:id', canActivate: [authGuard],
    loadComponent: () => import('./features/stories/story-detail/story-detail.component').then(m => m.StoryDetailComponent) },
  { path: 'stories/:id/edit', canActivate: [authGuard],
    loadComponent: () => import('./features/stories/story-form/story-form.component').then(m => m.StoryFormComponent) },
  // Tasks
  { path: 'stories/:storyId/tasks/new', canActivate: [authGuard],
    loadComponent: () => import('./features/tasks/task-form/task-form.component').then(m => m.TaskFormComponent) },
  { path: 'tasks/:id', canActivate: [authGuard],
    loadComponent: () => import('./features/tasks/task-detail/task-detail.component').then(m => m.TaskDetailComponent) },
  { path: 'tasks/:id/edit', canActivate: [authGuard],
    loadComponent: () => import('./features/tasks/task-form/task-form.component').then(m => m.TaskFormComponent) },
  // Subtasks
  { path: 'tasks/:taskId/subtasks/new', canActivate: [authGuard],
    loadComponent: () => import('./features/tasks/subtask-form/subtask-form.component').then(m => m.SubtaskFormComponent) },
  { path: 'subtasks/:id/edit', canActivate: [authGuard],
    loadComponent: () => import('./features/tasks/subtask-form/subtask-form.component').then(m => m.SubtaskFormComponent) },

  { path: '**', redirectTo: 'projects' }
];
