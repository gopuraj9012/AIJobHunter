import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { SidenavComponent } from './layout/sidenav.component';
import { HeaderComponent } from './layout/header.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    SidenavComponent,
    HeaderComponent,
  ],
  template: `
    <div class="app-container">
      <app-header></app-header>
      <mat-sidenav-container class="app-sidenav-container">
        <mat-sidenav mode="side" opened>
          <app-sidenav></app-sidenav>
        </mat-sidenav>
        <mat-sidenav-content>
          <main class="app-content">
            <router-outlet></router-outlet>
          </main>
        </mat-sidenav-content>
      </mat-sidenav-container>
    </div>
  `,
  styles: [`
    :host {
      display: flex;
      flex-direction: column;
      height: 100vh;
    }
    mat-sidenav {
      width: 260px;
      background: #ffffff;
      border-right: 1px solid #e0e0e0;
    }
    mat-sidenav-container {
      flex: 1;
      background: #f5f7fa;
    }
  `]
})
export class AppComponent {}