import { Component, EventEmitter, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ParsingService, UploadAndParseResult } from '../core/parsing.service';

@Component({
  selector: 'app-resume-upload',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <mat-card class="upload-card" appearance="outlined"
      [class.dragging]="isDragging"
      (dragover)="onDragOver($event)"
      (dragleave)="onDragLeave()"
      (drop)="onDrop($event)">
      <mat-card-content>
        <div class="upload-zone" *ngIf="!isParsing">
          <mat-icon class="upload-icon">cloud_upload</mat-icon>
          <h3>Upload your resume</h3>
          <p>Drag &amp; drop your PDF or DOCX here, or</p>
          <button mat-raised-button color="primary" type="button" (click)="fileInput.click()">
            <mat-icon>attach_file</mat-icon>
            Browse Files
          </button>
          <p class="hint">Supported formats: .pdf, .docx</p>
        </div>

        <div class="parsing-state" *ngIf="isParsing">
          <mat-spinner diameter="36" *ngIf="!fileName"></mat-spinner>
          <div class="file-chip" *ngIf="fileName">
            <mat-icon>description</mat-icon>
            <span>{{ fileName }}</span>
          </div>
          <p>{{ statusMessage }}</p>
          <mat-progress-bar mode="indeterminate"></mat-progress-bar>
        </div>
      </mat-card-content>

      <input #fileInput type="file" accept=".pdf,.docx"
        class="hidden-input" (change)="onFileSelected($event)" />
    </mat-card>
  `,
  styles: [`
    .upload-card {
      border: 2px dashed #bdbdbd;
      border-radius: 12px;
      transition: border-color 0.2s ease, background-color 0.2s ease;
    }
    .upload-card.dragging {
      border-color: #3f51b5;
      background-color: rgba(63, 81, 181, 0.06);
    }
    .upload-zone {
      text-align: center;
      padding: 32px 16px;
    }
    .upload-icon {
      font-size: 56px;
      width: 56px;
      height: 56px;
      color: #9e9e9e;
    }
    .upload-zone h3 {
      margin: 12px 0 4px;
      font-size: 1.2rem;
    }
    .upload-zone p {
      margin: 4px 0 16px;
      color: #666;
    }
    .hint {
      font-size: 0.8rem;
      color: #999;
    }
    .parsing-state {
      text-align: center;
      padding: 32px 16px;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 12px;
    }
    .parsing-state p {
      margin: 0;
      color: #555;
    }
    .file-chip {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      background: #e8eaf6;
      border-radius: 20px;
      padding: 6px 14px;
      font-size: 0.9rem;
    }
    .hidden-input {
      display: none;
    }
  `],
})
export class ResumeUploadComponent {
  private parsingService = inject(ParsingService);
  private snackBar = inject(MatSnackBar);

  /** Emitted with the uploaded Resume entity and AI-parsed sections once complete. */
  @Output() resumeParsed = new EventEmitter<UploadAndParseResult>();

  /** Emitted when the user wants to skip upload and build from scratch. */
  @Output() skipUpload = new EventEmitter<void>();

  isParsing = false;
  isDragging = false;
  fileName = '';
  statusMessage = '';

  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragging = true;
  }

  onDragLeave() {
    this.isDragging = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragging = false;
    const file = event.dataTransfer?.files?.[0];
    if (file) {
      this.processFile(file);
    }
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      this.processFile(file);
    }
    input.value = '';
  }

  private processFile(file: File) {
    const extension = file.name.split('.').pop()?.toLowerCase();
    if (extension !== 'pdf' && extension !== 'docx') {
      this.snackBar.open('Only PDF and DOCX files are supported', 'Close', { duration: 4000 });
      return;
    }

    this.fileName = file.name;
    this.isParsing = true;
    this.statusMessage = 'Uploading and extracting text...';

    this.parsingService.uploadAndParse(file).subscribe({
      next: (result) => {
        this.statusMessage = 'AI is structuring your resume...';
        this.resumeParsed.emit(result);
      },
      error: (err: Error) => {
        this.isParsing = false;
        this.fileName = '';
        const message =
          err?.message?.includes('File too large') || err?.message
            ? err.message
            : 'Failed to parse the file. Please try again.';
        this.snackBar.open(message, 'Close', { duration: 6000 });
      },
    });
  }
}
