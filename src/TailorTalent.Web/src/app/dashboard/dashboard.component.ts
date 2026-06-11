import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [MatCardModule, MatIconModule, MatButtonModule, RouterLink],
  template: `
    <div class="dashboard">
      <header class="dashboard-header">
        <h1>Welcome to TailorTalent</h1>
        <p>AI-powered resume optimization to land more interviews</p>
      </header>

      <div class="stats-grid">
        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-icon">
              <mat-icon color="primary">description</mat-icon>
            </div>
            <div class="stat-info">
              <span class="stat-value">0</span>
              <span class="stat-label">Resumes Created</span>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-icon">
              <mat-icon color="accent">auto_awesome</mat-icon>
            </div>
            <div class="stat-info">
              <span class="stat-value">0</span>
              <span class="stat-label">Optimizations</span>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-content>
            <div class="stat-icon">
              <mat-icon color="warn">work</mat-icon>
            </div>
            <div class="stat-info">
              <span class="stat-value">0</span>
              <span class="stat-label">Applications</span>
            </div>
          </mat-card-content>
        </mat-card>
      </div>

      <div class="action-cards">
        <mat-card class="action-card">
          <mat-card-header>
            <mat-icon mat-card-avatar>add_circle</mat-icon>
            <mat-card-title>Create New Resume</mat-card-title>
            <mat-card-subtitle>Start from scratch or upload an existing one</mat-card-subtitle>
          </mat-card-header>
          <mat-card-actions align="end">
            <button mat-raised-button color="primary" routerLink="/resumes/new">
              Get Started
            </button>
          </mat-card-actions>
        </mat-card>

        <mat-card class="action-card">
          <mat-card-header>
            <mat-icon mat-card-avatar>content_paste</mat-icon>
            <mat-card-title>Paste Job Description</mat-card-title>
            <mat-card-subtitle>Let AI tailor your resume to any job</mat-card-subtitle>
          </mat-card-header>
          <mat-card-actions align="end">
            <button mat-raised-button color="accent" routerLink="/resumes">
              Optimize Now
            </button>
          </mat-card-actions>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .dashboard {
      max-width: 1000px;
      margin: 0 auto;
    }
    .dashboard-header {
      margin-bottom: 32px;
    }
    .dashboard-header h1 {
      font-size: 2rem;
      font-weight: 700;
      margin: 0 0 8px 0;
      color: #1a1a2e;
    }
    .dashboard-header p {
      font-size: 1.1rem;
      color: #666;
      margin: 0;
    }
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 20px;
      margin-bottom: 32px;
    }
    .stat-card mat-card-content {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 20px;
    }
    .stat-icon mat-icon {
      font-size: 40px;
      height: 40px;
      width: 40px;
    }
    .stat-info {
      display: flex;
      flex-direction: column;
    }
    .stat-value {
      font-size: 2rem;
      font-weight: 700;
      color: #1a1a2e;
    }
    .stat-label {
      font-size: 0.9rem;
      color: #666;
    }
    .action-cards {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(340px, 1fr));
      gap: 20px;
    }
    .action-card {
      cursor: pointer;
      transition: transform 0.2s ease;
    }
    .action-card:hover {
      transform: translateY(-2px);
    }
  `]
})
export class DashboardComponent {}