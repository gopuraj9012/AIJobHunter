import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ResumeDto } from './resume.service';

/**
 * DTOs aligned with the .NET backend (ResumeParsingController + Program.cs
 * camelCase JSON policy):
 * - POST /api/parsing/extract  → ExtractTextResponse (500-char preview only)
 * - POST /api/parsing/upload   → ResumeDto (contains the FULL extracted text in rawContent)
 * - POST /api/parsing/parse    → ParseResumeResponse (structured sections from AI)
 */

/** Response from POST /api/parsing/extract */
export interface ExtractTextResponse {
  fileName: string;
  characterCount: number;
  preview: string;
}

/** Response from POST /api/parsing/parse — structured resume data for form pre-filling */
export interface ParseResumeResponse {
  personalInfo: PersonalInfoDto | null;
  summary: string | null;
  experience: ExperienceItemDto[] | null;
  education: EducationItemDto[] | null;
  skills: string[] | null;
}

export interface PersonalInfoDto {
  name: string | null;
  email: string | null;
  phone: string | null;
  location: string | null;
  linkedin: string | null;
  website: string | null;
}

export interface ExperienceItemDto {
  company: string | null;
  title: string | null;
  location: string | null;
  startDate: string | null;
  endDate: string | null;
  description: string | null;
  highlights: string[] | null;
}

export interface EducationItemDto {
  school: string | null;
  degree: string | null;
  location: string | null;
  graduationDate: string | null;
  description: string | null;
}

/** Result of the upload → parse pipeline */
export interface UploadAndParseResult {
  /** Resume entity created server-side by POST /api/parsing/upload (full text stored). */
  resume: ResumeDto;
  /** AI-structured sections from POST /api/parsing/parse. */
  parsed: ParseResumeResponse;
}

@Injectable({
  providedIn: 'root',
})
export class ParsingService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * Upload a PDF/DOCX file and extract its raw text.
   * NOTE: the backend returns only the first 500 characters as a preview;
   * use {@link uploadAndParse} for the full-text pipeline.
   */
  extractText(file: File): Observable<ExtractTextResponse> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<ExtractTextResponse>(`${this.apiUrl}/parsing/extract`, formData);
  }

  /**
   * Send raw resume text to the AI service to be structured into sections.
   * @param rawContent The extracted resume text (full text preferred).
   */
  parseResume(rawContent: string): Observable<ParseResumeResponse> {
    return this.http.post<ParseResumeResponse>(`${this.apiUrl}/parsing/parse`, { rawContent });
  }

  /**
   * Full pipeline: upload the file (which stores the FULL extracted text as a
   * Resume entity) and then parse that text into structured sections.
   *
   * @param file The PDF/DOCX resume file.
   * @param userId Owner id for the created Resume entity (no auth UI on main yet,
   *               so callers pass a stored user id or 'default-user').
   * @param title Optional resume title; defaults to the file name.
   */
  uploadAndParse(file: File, userId: string = 'default-user', title?: string): Observable<UploadAndParseResult> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    formData.append('userId', userId);
    formData.append('title', title ?? file.name.replace(/\.(pdf|docx)$/i, ''));

    return new Observable<UploadAndParseResult>((subscriber) => {
      this.http.post<ResumeDto>(`${this.apiUrl}/parsing/upload`, formData).subscribe({
        next: (resume) => {
          this.parseResume(resume.rawContent).subscribe({
            next: (parsed) => subscriber.next({ resume, parsed }),
            error: (err) => subscriber.error(err),
            complete: () => subscriber.complete(),
          });
        },
        error: (err) => subscriber.error(err),
      });
    });
  }
}
