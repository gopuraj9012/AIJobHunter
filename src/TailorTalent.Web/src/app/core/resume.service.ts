import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

/** Matches the backend ResumeDto record */
export interface ResumeDto {
  id: string;
  userId: string;
  title: string;
  rawContent: string;
  parsedSectionsJson: string;
  createdAt: string;
  updatedAt: string;
}

/** Matches the backend CreateResumeDto record */
export interface CreateResumeDto {
  userId: string;
  title: string;
  rawContent: string;
}

/** Matches the backend UpdateResumeDto record */
export interface UpdateResumeDto {
  title?: string;
  rawContent?: string;
  parsedSectionsJson?: string;
}

/** Parsed sections for the resume builder UI */
export interface ResumeSections {
  fullName: string;
  email: string;
  phone: string;
  linkedin: string;
  portfolio: string;
  experiences: Experience[];
  educations: Education[];
  skills: string[];
}

export interface Experience {
  company: string;
  role: string;
  startDate?: string;
  endDate?: string;
  description?: string;
}

export interface Education {
  school: string;
  degree: string;
  year?: string;
}

export interface TailorRequest {
  resumeId: string;
  jobDescriptionId: string;
  tone?: string;
}

export interface TailorResponse {
  sessionId: string;
  tailoredContent: string;
  atsScore: number;
  atsScoreBreakdown: ScoreBreakdown;
  missingKeywords: string[];
  highImpactMissingKeywords: string[];
  strengths: string[];
  weaknesses: string[];
  experienceBulletSuggestions: string[];
  improvementSuggestions: ImprovementSuggestion[];
}

export interface ScoreBreakdown {
  skills: number;
  experience: number;
  education: number;
}

export interface ImprovementSuggestion {
  section: string;
  feedback: string;
  suggestedRewrite: string;
}

@Injectable({
  providedIn: 'root',
})
export class ResumeService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** JWT token stored by the auth flow (localStorage key shared with future auth service). */
  private get authToken(): string | null {
    return localStorage.getItem('tailortalent.token');
  }

  /** Builds headers with the Bearer token when present (backend uses [Authorize]). */
  private authHeaders(): { Authorization?: string } {
    const token = this.authToken;
    return token ? { Authorization: `Bearer ${token}` } : {};
  }

  /** Get all resumes for the current user */
  getResumes(userId: string = 'default-user'): Observable<ResumeDto[]> {
    const params = new HttpParams().set('userId', userId);
    return this.http.get<ResumeDto[]>(`${this.apiUrl}/resumes`, { params, headers: this.authHeaders() });
  }

  /** Get a specific resume by ID */
  getResume(id: string): Observable<ResumeDto> {
    return this.http.get<ResumeDto>(`${this.apiUrl}/resumes/${id}`, { headers: this.authHeaders() });
  }

  /** Create a new resume */
  createResume(dto: CreateResumeDto): Observable<ResumeDto> {
    return this.http.post<ResumeDto>(`${this.apiUrl}/resumes`, dto, { headers: this.authHeaders() });
  }

  /** Update an existing resume (PATCH to match backend) */
  updateResume(id: string, dto: UpdateResumeDto): Observable<ResumeDto> {
    return this.http.patch<ResumeDto>(`${this.apiUrl}/resumes/${id}`, dto, { headers: this.authHeaders() });
  }

  /** Delete a resume */
  deleteResume(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/resumes/${id}`, { headers: this.authHeaders() });
  }

  /** Tailor a resume with AI — matches POST /api/tailoring/tailor */
  tailorResume(request: TailorRequest): Observable<TailorResponse> {
    return this.http.post<TailorResponse>(`${this.apiUrl}/tailoring/tailor`, request, { headers: this.authHeaders() });
  }

  /** Helper: serialise form sections to JSON for the API */
  serialiseSections(sections: ResumeSections): string {
    return JSON.stringify(sections);
  }

  /** Helper: parse sections JSON from the API */
  parseSections(json: string): ResumeSections | null {
    try {
      return JSON.parse(json) as ResumeSections;
    } catch {
      return null;
    }
  }
}