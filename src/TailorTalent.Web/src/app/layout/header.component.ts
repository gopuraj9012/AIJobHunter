import { Component } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [MatToolbarModule, MatIconModule, MatButtonModule],
  template: `
    <mat-toolbar color="primary">
      <span class="app-title">TailorTalent</span>
      <span class="app-spacer"></span>
      <button mat-icon-button aria-label="Notifications">
        <mat-icon>notifications</mat-icon>
      </button>
      <button mat-icon-button aria-label="User menu">
        <mat-icon>account_circle</mat-icon>
      </button>
    </mat-toolbar>
  `,
  styles: [`
    .app-title {
      font-weight: 700;
      font-size: 1.4rem;
      letter-spacing: -0.5px;
    }
    .app-spacer {
      flex: 1 1 auto;
    }
    mat-toolbar {
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.12);
      position: relative;
      z-index: 10;
    }
  `]
})
export class HeaderComponent {}