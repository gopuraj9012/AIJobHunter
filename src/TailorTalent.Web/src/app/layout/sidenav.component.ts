import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-sidenav',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatListModule, MatIconModule],
  template: `
    <div class="sidenav-header">
      <mat-icon class="sidenav-logo">auto_awesome</mat-icon>
      <span class="sidenav-brand">TailorTalent</span>
    </div>
    <mat-nav-list>
      <a mat-list-item routerLink="/dashboard" routerLinkActive="active-link">
        <mat-icon matListItemIcon>dashboard</mat-icon>
        <span matListItemTitle>Dashboard</span>
      </a>
      <a mat-list-item routerLink="/resumes" routerLinkActive="active-link">
        <mat-icon matListItemIcon>description</mat-icon>
        <span matListItemTitle>My Resumes</span>
      </a>
      <a mat-list-item routerLink="/resumes/new" routerLinkActive="active-link">
        <mat-icon matListItemIcon>add_circle</mat-icon>
        <span matListItemTitle>New Resume</span>
      </a>
    </mat-nav-list>
    <div class="sidenav-footer">
      <a mat-list-item>
        <mat-icon matListItemIcon>settings</mat-icon>
        <span matListItemTitle>Settings</span>
      </a>
      <a mat-list-item>
        <mat-icon matListItemIcon>help</mat-icon>
        <span matListItemTitle>Help</span>
      </a>
    </div>
  `,
  styles: [`
    .sidenav-header {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 20px 16px;
      border-bottom: 1px solid #e0e0e0;
    }
    .sidenav-logo {
      color: #3f51b5;
      font-size: 28px;
      height: 28px;
      width: 28px;
    }
    .sidenav-brand {
      font-weight: 700;
      font-size: 1.1rem;
      color: #1a1a2e;
    }
    .sidenav-footer {
      position: absolute;
      bottom: 0;
      width: 100%;
      border-top: 1px solid #e0e0e0;
    }
    .active-link {
      background-color: rgba(63, 81, 181, 0.12);
      color: #3f51b5;
      font-weight: 500;
    }
    mat-nav-list {
      padding-top: 8px;
    }
  `]
})
export class SidenavComponent {}