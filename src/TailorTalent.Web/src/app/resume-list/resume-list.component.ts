import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';

@Component({
  selector: 'app-resume-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatTableModule,
    MatChipsModule,
  ],
  template: `
    <div class="resume-list">
      <header class="page-header">
        <div>
          <h1>My Resumes</h1>
          <p>Manage and optimize your resumes</p>
        </div>
        <button mat-raised-button color="primary" routerLink="/resumes/new">
          <mat-icon>add</mat-icon>
          New Resume
        </button>
      </header>

      <mat-card>
        <mat-card-content>
          <div class="empty-state" *ngIf="resumes.length === 0">
            <mat-icon class="empty-icon">description</mat-icon>
            <h3>No resumes yet</h3>
            <p>Create your first resume to get started with AI-powered tailoring</p>
            <button mat-raised-button color="primary" routerLink="/resumes/new">
              Create Resume
            </button>
          </div>

          <table mat-table [dataSource]="resumes" *ngIf="resumes.length > 0" class="resume-table">
            <ng-container matColumnDef="name">
              <th mat-header-cell *matHeaderCellDef>Name</th>
              <td mat-cell *matCellDef="let resume">{{ resume.name }}</td>
            </ng-container>
            <ng-container matColumnDef="updated">
              <th mat-header-cell *matHeaderCellDef>Last Updated</th>
              <td mat-cell *matCellDef="let resume">{{ resume.updatedAt }}</td>
            </ng-container>
            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>Status</th>
              <td mat-cell *matCellDef="let resume">
                <mat-chip [color]="resume.optimized ? 'accent' : 'basic'" selected>
                  {{ resume.optimized ? 'Optimized' : 'Draft' }}
                </mat-chip>
              </td>
            </ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>Actions</th>
              <td mat-cell *matCellDef="let resume">
                <button mat-icon-button [routerLink]="['/resumes', resume.id, 'edit']">
                  <mat-icon>edit</mat-icon>
                </button>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .resume-list {
      max-width: 1000px;
      margin: 0 auto;
    }
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 24px;
    }
    .page-header h1 {
      font-size: 1.8rem;
      font-weight: 700;
      margin: 0 0 4px 0;
    }
    .page-header p {
      margin: 0;
      color: #666;
    }
    .empty-state {
      text-align: center;
      padding: 48px 24px;
    }
    .empty-icon {
      font-size: 64px;
      height: 64px;
      width: 64px;
      color: #bdbdbd;
      margin-bottom: 16px;
    }
    .empty-state h3 {
      font-size: 1.3rem;
      margin: 0 0 8px 0;
    }
    .empty-state p {
      color: #666;
      margin: 0 0 24px 0;
    }
    .resume-table {
      width: 100%;
    }
  `]
})
export class ResumeListComponent {
  resumes: any[] = [];
  displayedColumns = ['name', 'updated', 'status', 'actions'];
}