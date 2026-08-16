import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ResumeUploadComponent } from './resume-upload.component';
import { environment } from '../../environments/environment';

describe('ResumeUploadComponent', () => {
  let fixture: ComponentFixture<ResumeUploadComponent>;
  let component: ResumeUploadComponent;
  let httpMock: HttpTestingController;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;
  const api = environment.apiUrl;

  beforeEach(() => {
    snackBarSpy = jasmine.createSpyObj('MatSnackBar', ['open']);

    TestBed.configureTestingModule({
      imports: [ResumeUploadComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MatSnackBar, useValue: snackBarSpy },
      ],
    });

    fixture = TestBed.createComponent(ResumeUploadComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should render the upload prompt', () => {
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('Upload your resume');
    expect(element.textContent).toContain('PDF');
  });

  it('should reject non PDF/DOCX files with a snackbar message', () => {
    const file = new File(['x'], 'resume.txt', { type: 'text/plain' });
    component.onFileSelected({ target: { files: [file] } } as unknown as Event);

    expect(snackBarSpy.open).toHaveBeenCalledWith(
      'Only PDF and DOCX files are supported',
      jasmine.anything(),
      jasmine.anything()
    );
    expect(component.isParsing).toBeFalse();
  });

  it('should upload and emit parsed result for a valid PDF', () => {
    const file = new File(['resume text'], 'resume.pdf', { type: 'application/pdf' });
    let emitted: unknown;
    component.resumeParsed.subscribe((result) => (emitted = result));

    component.onFileSelected({ target: { files: [file] } } as unknown as Event);
    expect(component.isParsing).toBeTrue();
    expect(component.fileName).toBe('resume.pdf');

    // Upload request
    const uploadReq = httpMock.expectOne(`${api}/parsing/upload`);
    uploadReq.flush({
      id: 'r1',
      userId: 'default-user',
      title: 'resume',
      rawContent: 'resume text',
      parsedSectionsJson: '',
      createdAt: '2026-01-01T00:00:00',
      updatedAt: '2026-01-01T00:00:00',
    });

    // Parse request
    const parseReq = httpMock.expectOne(`${api}/parsing/parse`);
    parseReq.flush({
      personalInfo: { name: 'Jane Doe', email: 'jane@example.com', phone: null, location: null, linkedin: null, website: null },
      summary: null,
      experience: null,
      education: null,
      skills: ['Angular'],
    });

    expect(emitted).toBeDefined();
    expect((emitted as { resume: { id: string } }).resume.id).toBe('r1');
    expect((emitted as { parsed: { personalInfo: { name: string } | null } }).parsed.personalInfo?.name).toBe('Jane Doe');
  });

  it('should show an error snackbar when parsing fails', () => {
    const file = new File(['resume text'], 'resume.pdf', { type: 'application/pdf' });
    component.onFileSelected({ target: { files: [file] } } as unknown as Event);

    const uploadReq = httpMock.expectOne(`${api}/parsing/upload`);
    uploadReq.flush('Server error', { status: 500, statusText: 'Internal Server Error' });

    expect(snackBarSpy.open).toHaveBeenCalled();
    expect(component.isParsing).toBeFalse();
    expect(component.fileName).toBe('');
  });

  it('should handle drop events with a file', () => {
    const file = new File(['resume text'], 'resume.docx', { type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' });
    const dropEvent = {
      preventDefault: jasmine.createSpy('preventDefault'),
      dataTransfer: { files: [file] },
    } as unknown as DragEvent;

    component.onDrop(dropEvent);

    expect(dropEvent.preventDefault).toHaveBeenCalled();
    expect(component.fileName).toBe('resume.docx');
    expect(component.isParsing).toBeTrue();

    const uploadReq = httpMock.expectOne(`${api}/parsing/upload`);
    uploadReq.flush({
      id: 'r3',
      userId: 'default-user',
      title: 'resume',
      rawContent: 'resume text',
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
});
