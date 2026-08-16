import { Component, OnInit, inject } from '@angular/core';
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
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ResumeService, ResumeSections } from '../core/resume.service';
import { ParsingService, UploadAndParseResult } from '../core/parsing.service';
import { ResumeUploadComponent } from './resume-upload.component';

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
    MatTabsModule,
    MatProgressBarModule,
    ResumeUploadComponent,
  ],
  template: `
    <div class="resume-builder">
      <header class="page-header">
        <div>
          <h1>{{ isEditing ? 'Edit Resume' : 'Create New Resume' }}</h1>
          <p>Build and tailor your resume with AI-powered suggestions</p>
        </div>
        <div class="header-actions">
          <button mat-stroked-button (click)="analyzeJobDescription()" [disabled]="!jobDescription || saving">
            <mat-icon>auto_awesome</mat-icon>
            Analyze with AI
          </button>
        </div>
      </header>

      <!-- Resume Upload (new resumes only, until parsed or skipped) -->
      <div class="upload-section" *ngIf="showUpload">
        <app-resume-upload
          (resumeParsed)="onResumeParsed($event)"
          (skipUpload)="showUpload = false">
        </app-resume-upload>
        <div class="upload-or">
          <mat-divider></mat-divider>
          <span>or</span>
          <mat-divider></mat-divider>
        </div>
        <button mat-button color="primary" (click)="showUpload = false">
          Start from scratch
        </button>
      </div>

      <div class="builder-layout" *ngIf="!showUpload">
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
                          <input matInput [formControl]="$any(exp.get('company'))" placeholder="Acme Corp" />
                        </mat-form-field>
                        <mat-form-field appearance="outline">
                          <mat-label>Role</mat-label>
                          <input matInput [formControl]="$any(exp.get('role'))" placeholder="Software Engineer" />
                        </mat-form-field>
                      </div>
                      <div class="form-row">
                        <mat-form-field appearance="outline">
                          <mat-label>Start Date</mat-label>
                          <input matInput [formControl]="$any(exp.get('startDate'))" placeholder="Jan 2020" />
                        </mat-form-field>
                        <mat-form-field appearance="outline">
                          <mat-label>End Date</mat-label>
                          <input matInput [formControl]="$any(exp.get('endDate'))" placeholder="Present" />
                        </mat-form-field>
                      </div>
                      <mat-form-field appearance="outline">
                        <mat-label>Description</mat-label>
                        <textarea matInput rows="3" [formControl]="$any(exp.get('description'))"
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
                          <input matInput [formControl]="$any(edu.get('school'))" placeholder="University" />
                        </mat-form-field>
                        <mat-form-field appearance="outline">
                          <mat-label>Degree</mat-label>
                          <input matInput [formControl]="$any(edu.get('degree'))" placeholder="B.S. Computer Science" />
                        </mat-form-field>
                      </div>
                      <div class="form-row">
                        <mat-form-field appearance="outline">
                          <mat-label>Graduation Year</mat-label>
                          <input matInput [formControl]="$any(edu.get('year'))" placeholder="2024" />
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
            <button mat-stroked-button routerLink="/resumes" [disabled]="saving">Cancel</button>
            <button mat-raised-button color="primary" (click)="saveResume()" [disabled]="!isFormValid() || saving">
              <mat-icon>save</mat-icon>
              {{ saving ? 'Saving...' : 'Save Resume' }}
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
    .upload-section {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 16px;
      margin-bottom: 24px;
    }
    .upload-section app-resume-upload {
      width: 100%;
      max-width: 640px;
    }
    .upload-or {
      display: flex;
      align-items: center;
      gap: 12px;
      width: 100%;
      max-width: 640px;
      color: #999;
      font-size: 0.85rem;
    }
    .upload-or mat-divider {
      flex: 1;
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
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);
  private resumeService = inject(ResumeService);

  isEditing = false;
  resumeId: string | null = null;
  jobDescription = '';
  showUpload = true;
  saving = false;

  // Form controls
  fullName = this.fb.control('', Validators.required);
  email = this.fb.control('', [Validators.required, Validators.email]);
  phone = this.fb.control('');
  linkedin = this.fb.control('');
  portfolio = this.fb.control('');
  experiences = this.fb.array<FormGroup>([]);
  educations = this.fb.array<FormGroup>([]);
  skills = this.fb.array<string[]>([]);

  constructor() {}

  ngOnInit() {
    this.resumeId = this.route.snapshot.paramMap.get('id');
    this.isEditing = !!this.resumeId;
    this.showUpload = !this.isEditing;
    if (this.isEditing && this.resumeId) {
      this.loadResume(this.resumeId);
    }
  }

  /** Load an existing resume from the API and populate the form. */
  private loadResume(id: string) {
    this.resumeService.getResume(id).subscribe({
      next: (resume) => {
        this.applySectionsJson(resume.parsedSectionsJson);
        this.snackBar.open('Resume loaded', 'Close', { duration: 2000 });
      },
      error: () => {
        this.snackBar.open('Failed to load resume from server', 'Close', { duration: 4000 });
      },
    });
  }

  /** Map saved sections JSON (ResumeSections shape) onto the form controls. */
  private applySectionsJson(json: string | null) {
    if (!json) return;
    const sections = this.resumeService.parseSections(json);
    if (!sections) return;
    this.fullName.setValue(sections.fullName ?? '');
    this.email.setValue(sections.email ?? '');
    this.phone.setValue(sections.phone ?? '');
    this.linkedin.setValue(sections.linkedin ?? '');
    this.portfolio.setValue(sections.portfolio ?? '');
    this.experiences.clear();
    for (const exp of sections.experiences ?? []) {
      this.pushExperience(exp.company, exp.role, exp.startDate ?? '', exp.endDate ?? '', exp.description ?? '');
    }
    this.educations.clear();
    for (const edu of sections.educations ?? []) {
      this.pushEducation(edu.school, edu.degree, edu.year ?? '');
    }
    this.skills.clear();
    for (const skill of sections.skills ?? []) {
      if (skill?.trim()) this.skills.push(this.fb.control(skill.trim()));
    }
  }

  /**
   * Upload → AI parse completed: pre-fill all four form tabs with the parsed data.
   * The upload endpoint already created the Resume entity server-side, so the
   * builder switches to edit mode for that resume id.
   */
  onResumeParsed(result: UploadAndParseResult) {
    const parsed = result.parsed;

    if (result.resume?.id) {
      this.resumeId = result.resume.id;
      this.isEditing = true;
    }

    const info = parsed.personalInfo;
    if (info) {
      this.fullName.setValue(info.name ?? '');
      this.email.setValue(info.email ?? '');
      this.phone.setValue(info.phone ?? '');
      this.linkedin.setValue(info.linkedin ?? '');
      this.portfolio.setValue(info.website ?? '');
    }

    this.experiences.clear();
    for (const exp of parsed.experience ?? []) {
      const descParts = [exp.description ?? '', ...(exp.highlights ?? [])].filter(
        (p) => p && p.trim()
      );
      this.pushExperience(
        exp.company ?? '',
        exp.title ?? '',
        exp.startDate ?? '',
        exp.endDate ?? '',
        descParts.join('\n')
      );
    }

    this.educations.clear();
    for (const edu of parsed.education ?? []) {
      this.pushEducation(edu.school ?? '', edu.degree ?? '', edu.graduationDate ?? '');
    }

    this.skills.clear();
    for (const skill of parsed.skills ?? []) {
      if (skill?.trim()) this.skills.push(this.fb.control(skill.trim()));
    }

    this.showUpload = false;
    this.snackBar.open('Resume parsed! Review and customize the sections below.', 'Close', { duration: 4000 });
  }

  private pushExperience(company: string, role: string, startDate: string, endDate: string, description: string) {
    const group = this.fb.group({
      company: [company, Validators.required],
      role: [role, Validators.required],
      startDate: [startDate],
      endDate: [endDate],
      description: [description],
    });
    this.experiences.push(group);
  }

  private pushEducation(school: string, degree: string, year: string) {
    const group = this.fb.group({
      school: [school, Validators.required],
      degree: [degree, Validators.required],
      year: [year],
    });
    this.educations.push(group);
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
    this.pushExperience('', '', '', '', '');
  }

  removeExperience(index: number) {
    this.experiences.removeAt(index);
  }

  addEducation() {
    this.pushEducation('', '', '');
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

  /** Collect the current form values into the ResumeSections shape used by the API. */
  private collectSections(): ResumeSections {
    return {
      fullName: this.fullName.value ?? '',
      email: this.email.value ?? '',
      phone: this.phone.value ?? '',
      linkedin: this.linkedin.value ?? '',
      portfolio: this.portfolio.value ?? '',
      experiences: this.experiences.value ?? [],
      educations: this.educations.value ?? [],
      skills: this.skills.value.map((s) => s ?? '') ?? [],
    };
  }

  /** Persist the resume: update when an id exists (upload-created or edit), create otherwise. */
  saveResume() {
    if (!this.isFormValid()) return;

    this.saving = true;
    const sections = this.collectSections();
    const parsedJson = this.resumeService.serialiseSections(sections);
    const title = sections.fullName?.trim() || 'Untitled Resume';
    const userId = localStorage.getItem('tailortalent.userId') ?? 'default-user';

    const request$ = this.resumeId
      ? this.resumeService.updateResume(this.resumeId, {
          title,
          parsedSectionsJson: parsedJson,
        })
      : this.resumeService.createResume({
          userId,
          title,
          rawContent: parsedJson,
        });

    request$.subscribe({
      next: () => {
        this.saving = false;
        this.snackBar.open(this.isEditing ? 'Resume updated!' : 'Resume created!', 'Close', { duration: 3000 });
        this.router.navigate(['/resumes']);
      },
      error: (err) => {
        this.saving = false;
        const message =
          err?.status === 401
            ? 'Please log in to save your resume.'
            : 'Failed to save resume. Please try again.';
        this.snackBar.open(message, 'Close', { duration: 5000 });
      },
    });
  }

  analyzeJobDescription() {
    if (!this.jobDescription.trim()) {
      this.snackBar.open('Please paste a job description first', 'Close', { duration: 3000 });
      return;
    }
    // TODO: Call AI analysis API (POST /api/tailoring/analyze)
    this.snackBar.open('AI analysis started...', 'Close', { duration: 2000 });
  }
}
