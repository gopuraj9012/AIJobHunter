import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressBarModule } from '@angular/material/progress-bar';

@Component({
  selector: 'app-resume-builder',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatChipsModule,
    MatSnackBarModule,
    MatTabsModule,
    MatProgressBarModule,
  ],
  template: `
    <div class="resume-builder">
      <header class="page-header">
        <div>
          <h1>{{ isEditing ? 'Edit Resume' : 'Create New Resume' }}</h1>
          <p>Build and tailor your resume with AI-powered suggestions</p>
        </div>
        <div class="header-actions">
          <button mat-stroked-button (click)="analyzeJobDescription()" [disabled]="!jobDescription">
            <mat-icon>auto_awesome</mat-icon>
            Analyze with AI
          </button>
        </div>
      </header>

      <div class="builder-layout">
        <!-- Job Description Panel -->
        <mat-card class="jd-panel">
          <mat-card-header>
            <mat-icon mat-card-avatar>content_paste</mat-icon>
            <mat-card-title>Job Description</mat-card-title>
            <mat-card-subtitle>Paste the target job description here</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <mat-form-field appearance="outline">
              <textarea
                matInput
                rows="8"
                placeholder="Paste the full job description here to get AI-powered tailoring suggestions..."
                [(ngModel)]="jobDescription"
              ></textarea>
            </mat-form-field>
            <div class="jd-stats" *ngIf="jobDescription">
              <mat-chip>{{ keywordCount }} keywords detected</mat-chip>
              <mat-chip color="accent" selected>
                {{ missingKeywords }} missing from resume
              </mat-chip>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- Resume Form -->
        <mat-card class="resume-form-card">
          <mat-tab-group dynamicHeight>
            <!-- Personal Info Tab -->
            <mat-tab label="Personal Info">
              <div class="tab-content">
                <div class="form-row">
                  <mat-form-field appearance="outline">
                    <mat-label>Full Name</mat-label>
                    <input matInput placeholder="John Doe" [formControl]="fullName" />
                  </mat-form-field>
                </div>
                <div class="form-row">
                  <mat-form-field appearance="outline">
                    <mat-label>Email</mat-label>
                    <input matInput placeholder="john@example.com" [formControl]="email" />
                  </mat-form-field>
                  <mat-form-field appearance="outline">
                    <mat-label>Phone</mat-label>
                    <input matInput placeholder="+1 (555) 123-4567" [formControl]="phone" />
                  </mat-form-field>
                </div>
                <div class="form-row">
                  <mat-form-field appearance="outline">
                    <mat-label>LinkedIn URL</mat-label>
                    <input matInput placeholder="https://linkedin.com/in/johndoe" [formControl]="linkedin" />
                  </mat-form-field>
                  <mat-form-field appearance="outline">
                    <mat-label>Portfolio URL</mat-label>
                    <input matInput placeholder="https://johndoe.com" [formControl]="portfolio" />
                  </mat-form-field>
                </div>
              </div>
            </mat-tab>

            <!-- Experience Tab -->
            <mat-tab label="Experience">
              <div class="tab-content">
                <div class="section-header">
                  <h3>Work Experience</h3>
                  <button mat-mini-fab color="primary" (click)="addExperience()">
                    <mat-icon>add</mat-icon>
                  </button>
                </div>
                <div *ngFor="let exp of experiences.controls; let i = index" class="repeated-section">
                  <mat-card class="experience-card" appearance="outlined">
                    <mat-card-header>
                      <mat-card-title>Experience #{{ i + 1 }}</mat-card-title>
                      <button mat-icon-button color="warn" (click)="removeExperience(i)">
                        <mat-icon>delete</mat-icon>
                      </button>
                    </mat-card-header>
                    <mat-card-content>
                      <div class="form-row">
                        <mat-form-field appearance="outline">
                          <mat-label>Company</mat-label>
                          <input matInput [formControl]="exp.get('company')" placeholder="Acme Corp" />
                        </mat-form-field>
                        <mat-form-field appearance="outline">
                          <mat-label>Role</mat-label>
                          <input matInput [formControl]="exp.get('role')" placeholder="Software Engineer" />
                        </mat-form-field>
                      </div>
                      <div class="form-row">
                        <mat-form-field appearance="outline">
                          <mat-label>Start Date</mat-label>
                          <input matInput [formControl]="exp.get('startDate')" placeholder="Jan 2020" />
                        </mat-form-field>
                        <mat-form-field appearance="outline">
                          <mat-label>End Date</mat-label>
                          <input matInput [formControl]="exp.get('endDate')" placeholder="Present" />
                        </mat-form-field>
                      </div>
                      <mat-form-field appearance="outline">
                        <mat-label>Description</mat-label>
                        <textarea matInput rows="3" [formControl]="exp.get('description')"
                          placeholder="Describe your responsibilities and achievements..."></textarea>
                      </mat-form-field>
                    </mat-card-content>
                  </mat-card>
                </div>
                <div class="empty-hint" *ngIf="experiences.length === 0">
                  <p>Add your work experience to get started</p>
                </div>
              </div>
            </mat-tab>

            <!-- Education Tab -->
            <mat-tab label="Education">
              <div class="tab-content">
                <div class="section-header">
                  <h3>Education</h3>
                  <button mat-mini-fab color="primary" (click)="addEducation()">
                    <mat-icon>add</mat-icon>
                  </button>
                </div>
                <div *ngFor="let edu of educations.controls; let i = index" class="repeated-section">
                  <mat-card appearance="outlined">
                    <mat-card-header>
                      <mat-card-title>Education #{{ i + 1 }}</mat-card-title>
                      <button mat-icon-button color="warn" (click)="removeEducation(i)">
                        <mat-icon>delete</mat-icon>
                      </button>
                    </mat-card-header>
                    <mat-card-content>
                      <div class="form-row">
                        <mat-form-field appearance="outline">
                          <mat-label>School</mat-label>
                          <input matInput [formControl]="edu.get('school')" placeholder="University" />
                        </mat-form-field>
                        <mat-form-field appearance="outline">
                          <mat-label>Degree</mat-label>
                          <input matInput [formControl]="edu.get('degree')" placeholder="B.S. Computer Science" />
                        </mat-form-field>
                      </div>
                      <div class="form-row">
                        <mat-form-field appearance="outline">
                          <mat-label>Graduation Year</mat-label>
                          <input matInput [formControl]="edu.get('year')" placeholder="2024" />
                        </mat-form-field>
                      </div>
                    </mat-card-content>
                  </mat-card>
                </div>
              </div>
            </mat-tab>

            <!-- Skills Tab -->
            <mat-tab label="Skills">
              <div class="tab-content">
                <div class="section-header">
                  <h3>Skills</h3>
                  <button mat-mini-fab color="primary" (click)="addSkill()">
                    <mat-icon>add</mat-icon>
                  </button>
                </div>
                <div class="chips-collection">
                  <mat-chip *ngFor="let skill of skills.controls; let i = index" (removed)="removeSkill(i)">
                    {{ skill.value }}
                    <button matChipRemove>
                      <mat-icon>cancel</mat-icon>
                    </button>
                  </mat-chip>
                </div>
                <div class="add-skill-row">
                  <mat-form-field appearance="outline" class="skill-input">
                    <mat-label>Add Skill</mat-label>
                    <input matInput #skillInput placeholder="TypeScript, Angular, React..." />
                  </mat-form-field>
                  <button mat-raised-button (click)="addSkillFromInput(skillInput.value); skillInput.value=''">
                    Add
                  </button>
                </div>
              </div>
            </mat-tab>
          </mat-tab-group>

          <mat-card-actions align="end" class="form-actions">
            <button mat-stroked-button routerLink="/resumes">Cancel</button>
            <button mat-raised-button color="primary" (click)="saveResume()" [disabled]="!isFormValid()">
              <mat-icon>save</mat-icon>
              Save Resume
            </button>
          </mat-card-actions>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .resume-builder {
      max-width: 1100px;
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
    .builder-layout {
      display: grid;
      grid-template-columns: 1fr 2fr;
      gap: 24px;
    }
    @media (max-width: 768px) {
      .builder-layout {
        grid-template-columns: 1fr;
      }
    }
    .jd-panel mat-card-header {
      margin-bottom: 16px;
    }
    .jd-stats {
      display: flex;
      gap: 8px;
      margin-top: 12px;
    }
    .tab-content {
      padding: 20px 0;
    }
    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
      margin-bottom: 4px;
    }
    .form-row:has(mat-form-field:only-child) {
      grid-template-columns: 1fr;
    }
    .section-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
    }
    .section-header h3 {
      margin: 0;
      font-size: 1.1rem;
    }
    .repeated-section {
      margin-bottom: 16px;
    }
    .experience-card mat-card-header {
      display: flex;
      justify-content: space-between;
    }
    .experience-card mat-card-header button {
      margin-left: auto;
    }
    .empty-hint {
      text-align: center;
      padding: 32px;
      color: #999;
    }
    .chips-collection {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      margin-bottom: 16px;
      min-height: 48px;
    }
    .add-skill-row {
      display: flex;
      gap: 12px;
      align-items: flex-start;
    }
    .skill-input {
      flex: 1;
    }
    .form-actions {
      padding: 16px 24px !important;
      gap: 12px;
      border-top: 1px solid #e0e0e0;
    }
  `]
})
export class ResumeBuilderComponent implements OnInit {
  isEditing = false;
  resumeId: string | null = null;
  jobDescription = '';

  // Form controls
  fullName = this.fb.control('', Validators.required);
  email = this.fb.control('', [Validators.required, Validators.email]);
  phone = this.fb.control('');
  linkedin = this.fb.control('');
  portfolio = this.fb.control('');
  experiences = this.fb.array<FormGroup>([]);
  educations = this.fb.array<FormGroup>([]);
  skills = this.fb.array<string[]>([]);

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.resumeId = this.route.snapshot.paramMap.get('id');
    this.isEditing = !!this.resumeId;
    if (this.isEditing) {
      // TODO: Load resume data from API
    }
  }

  get keywordCount(): number {
    if (!this.jobDescription) return 0;
    const words = this.jobDescription.split(/\s+/).filter(w => w.length > 2);
    return words.length;
  }

  get missingKeywords(): number {
    // Placeholder - will be replaced with AI analysis
    return 0;
  }

  addExperience() {
    this.experiences.push(this.fb.group({
      company: ['', Validators.required],
      role: ['', Validators.required],
      startDate: [''],
      endDate: [''],
      description: ['']
    }));
  }

  removeExperience(index: number) {
    this.experiences.removeAt(index);
  }

  addEducation() {
    this.educations.push(this.fb.group({
      school: ['', Validators.required],
      degree: ['', Validators.required],
      year: ['']
    }));
  }

  removeEducation(index: number) {
    this.educations.removeAt(index);
  }

  addSkill() {
    // Handled by addSkillFromInput
  }

  addSkillFromInput(value: string) {
    if (value.trim()) {
      this.skills.push(this.fb.control(value.trim()));
    }
  }

  removeSkill(index: number) {
    this.skills.removeAt(index);
  }

  isFormValid(): boolean {
    return this.fullName.valid && this.email.valid;
  }

  saveResume() {
    if (!this.isFormValid()) return;

    const resumeData = {
      fullName: this.fullName.value,
      email: this.email.value,
      phone: this.phone.value,
      linkedin: this.linkedin.value,
      portfolio: this.portfolio.value,
      experiences: this.experiences.value,
      educations: this.educations.value,
      skills: this.skills.value,
      jobDescription: this.jobDescription,
    };

    console.log('Saving resume:', resumeData);
    // TODO: Call API to save resume

    this.snackBar.open(
      this.isEditing ? 'Resume updated!' : 'Resume created!',
      'Close',
      { duration: 3000 }
    );
  }

  analyzeJobDescription() {
    if (!this.jobDescription.trim()) {
      this.snackBar.open('Please paste a job description first', 'Close', { duration: 3000 });
      return;
    }
    // TODO: Call AI analysis API
    this.snackBar.open('AI analysis started...', 'Close', { duration: 2000 });
  }
}