import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ParsingService } from './parsing.service';
import { environment } from '../../environments/environment';

describe('ParsingService', () => {
  let service: ParsingService;
  let httpMock: HttpTestingController;
  const api = environment.apiUrl;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), ParsingService],
    });
    service = TestBed.inject(ParsingService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('extractText should POST a FormData file to /parsing/extract', () => {
    const file = new File(['resume text'], 'resume.pdf', { type: 'application/pdf' });

    service.extractText(file).subscribe((res) => {
      expect(res.fileName).toBe('resume.pdf');
      expect(res.characterCount).toBe(11);
    });

    const req = httpMock.expectOne(`${api}/parsing/extract`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body instanceof FormData).toBeTrue();
    expect((req.request.body.get('file') as File).name).toBe('resume.pdf');
    req.flush({ fileName: 'resume.pdf', characterCount: 11, preview: 'resume text' });
  });

  it('parseResume should POST raw content to /parsing/parse', () => {
    service.parseResume('John Doe email@x.com').subscribe((res) => {
      expect(res.personalInfo?.name).toBe('John Doe');
      expect(res.skills).toEqual(['Angular', 'TypeScript']);
    });

    const req = httpMock.expectOne(`${api}/parsing/parse`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ rawContent: 'John Doe email@x.com' });
    req.flush({
      personalInfo: { name: 'John Doe', email: null, phone: null, location: null, linkedin: null, website: null },
      summary: null,
      experience: null,
      education: null,
      skills: ['Angular', 'TypeScript'],
    });
  });

  it('uploadAndParse should chain upload then parse', () => {
    const file = new File(['full resume text'], 'resume.docx', { type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' });

    service.uploadAndParse(file, 'user-1', 'My Resume').subscribe((result) => {
      expect(result.resume.id).toBe('r1');
      expect(result.resume.rawContent).toBe('full resume text');
      expect(result.parsed.personalInfo?.email).toBe('john@example.com');
    });

    // Upload request
    const uploadReq = httpMock.expectOne(`${api}/parsing/upload`);
    expect(uploadReq.request.method).toBe('POST');
    const formData = uploadReq.request.body as FormData;
    expect((formData.get('file') as File).name).toBe('resume.docx');
    expect(formData.get('userId')).toBe('user-1');
    expect(formData.get('title')).toBe('My Resume');
    uploadReq.flush({
      id: 'r1',
      userId: 'user-1',
      title: 'My Resume',
      rawContent: 'full resume text',
      parsedSectionsJson: '',
      createdAt: '2026-01-01T00:00:00',
      updatedAt: '2026-01-01T00:00:00',
    });

    // Parse request triggered by the upload response
    const parseReq = httpMock.expectOne(`${api}/parsing/parse`);
    expect(parseReq.request.method).toBe('POST');
    expect(parseReq.request.body).toEqual({ rawContent: 'full resume text' });
    parseReq.flush({
      personalInfo: { name: 'John Doe', email: 'john@example.com', phone: null, location: null, linkedin: null, website: null },
      summary: null,
      experience: null,
      education: null,
      skills: null,
    });
  });

  it('uploadAndParse should default the title to the file name', () => {
    const file = new File(['x'], 'john.pdf', { type: 'application/pdf' });

    service.uploadAndParse(file).subscribe();
    const uploadReq = httpMock.expectOne(`${api}/parsing/upload`);
    const formData = uploadReq.request.body as FormData;
    expect(formData.get('userId')).toBe('default-user');
    expect(formData.get('title')).toBe('john');
    uploadReq.flush({
      id: 'r2',
      userId: 'default-user',
      title: 'john',
      rawContent: 'x',
      parsedSectionsJson: '',
      createdAt: '2026-01-01T00:00:00',
      updatedAt: '2026-01-01T00:00:00',
    });

    const parseReq = httpMock.expectOne(`${api}/parsing/parse`);
    parseReq.flush({
      personalInfo: null,
      summary: null,
      experience: null,
      education: null,
      skills: null,
    });
  });

  it('uploadAndParse should surface upload errors', () => {
    const file = new File(['x'], 'bad.pdf', { type: 'application/pdf' });
    let error: Error | undefined;

    service.uploadAndParse(file).subscribe({
      error: (err) => (error = err),
    });

    const uploadReq = httpMock.expectOne(`${api}/parsing/upload`);
    uploadReq.flush('Upload failed', { status: 400, statusText: 'Bad Request' });

    expect(error).toBeDefined();
  });
});
