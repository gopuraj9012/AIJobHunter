import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ResumeListComponent } from './resume-list/resume-list.component';
import { ResumeBuilderComponent } from './resume-builder/resume-builder.component';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'resumes', component: ResumeListComponent },
  { path: 'resumes/new', component: ResumeBuilderComponent },
  { path: 'resumes/upload', component: ResumeBuilderComponent },
  { path: 'resumes/:id/edit', component: ResumeBuilderComponent },
  { path: '**', redirectTo: '/dashboard' },
];