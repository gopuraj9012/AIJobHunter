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
  jobDescription: string;
}

export interface TailorResponse {
  optimizedResume: ResumeDto;
  suggestions: string[];
  matchedKeywords: string[];
  missingKeywords: string[];
  matchScore: number;
}

@Injectable({
  providedIn: 'root',
})
export class ResumeService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** Get all resumes for the current user */
  getResumes(userId: string = 'default-user'): Observable<ResumeDto[]> {
    const params = new HttpParams().set('userId', userId);
    return this.http.get<ResumeDto[]>(`${this.apiUrl}/resumes`, { params });
  }

  /** Get a specific resume by ID */
  getResume(id: string): Observable<ResumeDto> {
    return this.http.get<ResumeDto>(`${this.apiUrl}/resumes/${id}`);
  }

  /** Create a new resume */
  createResume(dto: CreateResumeDto): Observable<ResumeDto> {
    return this.http.post<ResumeDto>(`${this.apiUrl}/resumes`, dto);
  }

  /** Update an existing resume (PATCH to match backend) */
  updateResume(id: string, dto: UpdateResumeDto): Observable<ResumeDto> {
    return this.http.patch<ResumeDto>(`${this.apiUrl}/resumes/${id}`, dto);
  }

  /** Delete a resume */
  deleteResume(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/resumes/${id}`);
  }

  /** Tailor a resume with AI */
  tailorResume(request: TailorRequest): Observable<TailorResponse> {
    return this.http.post<TailorResponse>(`${this.apiUrl}/resumes/tailor`, request);
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