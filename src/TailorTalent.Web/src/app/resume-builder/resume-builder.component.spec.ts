import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ResumeBuilderComponent } from './resume-builder.component';
import { UploadAndParseResult } from '../core/parsing.service';
import { environment } from '../../environments/environment';

describe('ResumeBuilderComponent', () => {
  let fixture: ComponentFixture<ResumeBuilderComponent>;
  let component: ResumeBuilderComponent;
  let httpMock: HttpTestingController;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;
  let routerSpy: jasmine.SpyObj<Router>;
  const api = environment.apiUrl;

  const parsedResult: UploadAndParseResult = {
    resume: {
      id: 'parsed-resume-id',
      userId: 'default-user',
      title: 'resume',
      rawContent: 'full resume text',
      parsedSectionsJson: '',
      createdAt: '2026-01-01T00:00:00',
      updatedAt: '2026-01-01T00:00:00',
    },
    parsed: {
      personalInfo: {
        name: 'Jane Doe',
        email: 'jane@example.com',
        phone: '+1 555 000 1234',
        location: 'San Francisco',
        linkedin: 'https://linkedin.com/in/janedoe',
        website: 'https://janedoe.dev',
      },
      summary: 'Senior engineer',
      experience: [
        {
          company: 'Acme Corp',
          title: 'Senior Software Engineer',
          location: 'Remote',
          startDate: '2020-01',
          endDate: 'Present',
          description: 'Built scalable services',
          highlights: ['Led 3-person team', 'Shipped 10 features'],
        },
      ],
      education: [
        { school: 'State University', degree: 'B.S. Computer Science', location: 'NY', graduationDate: '2019', description: null },
      ],
      skills: ['Angular', 'TypeScript', 'RxJS'],
    },
  };

  beforeEach(() => {
    snackBarSpy = jasmine.createSpyObj('MatSnackBar', ['open']);
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      imports: [ResumeBuilderComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MatSnackBar, useValue: snackBarSpy },
        { provide: Router, useValue: routerSpy },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => null } } },
        },
      ],
    });

    fixture = TestBed.createComponent(ResumeBuilderComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should render the upload step for new resumes', () => {
    expect(component.showUpload).toBeTrue();
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('Upload your resume');
  });

  it('should auto-populate all form tabs from the parsed result', () => {
    component.onResumeParsed(parsedResult);

    // Personal info
    expect(component.fullName.value).toBe('Jane Doe');
    expect(component.email.value).toBe('jane@example.com');
    expect(component.phone.value).toBe('+1 555 000 1234');
    expect(component.linkedin.value).toBe('https://linkedin.com/in/janedoe');
    expect(component.portfolio.value).toBe('https://janedoe.dev');

    // Experience
    expect(component.experiences.length).toBe(1);
    const exp = component.experiences.at(0);
    expect(exp.get('company')?.value).toBe('Acme Corp');
    expect(exp.get('role')?.value).toBe('Senior Software Engineer');
    expect(exp.get('startDate')?.value).toBe('2020-01');
    expect(exp.get('endDate')?.value).toBe('Present');
    expect(exp.get('description')?.value).toContain('Built scalable services');
    expect(exp.get('description')?.value).toContain('Led 3-person team');

    // Education
    expect(component.educations.length).toBe(1);
    const edu = component.educations.at(0);
    expect(edu.get('school')?.value).toBe('State University');
    expect(edu.get('degree')?.value).toBe('B.S. Computer Science');
    expect(edu.get('year')?.value).toBe('2019');

    // Skills
    expect(component.skills.length).toBe(3);
    expect(component.skills.at(0).value).toBe('Angular');

    // Builder switches to edit mode (resume exists server-side)
    expect(component.isEditing).toBeTrue();
    expect(component.resumeId).toBe('parsed-resume-id');
    expect(component.showUpload).toBeFalse();
  });

  it('should create a resume via the API when saving a new resume', () => {
    component.showUpload = false;
    component.fullName.setValue('Jane Doe');
    component.email.setValue('jane@example.com');
    component.addSkillFromInput('Angular');

    component.saveResume();

    const req = httpMock.expectOne(`${api}/resumes`);
    expect(req.request.method).toBe('POST');
    const body = req.request.body as { userId: string; title: string; rawContent: string };
    expect(body.userId).toBe('default-user');
    expect(body.title).toBe('Jane Doe');
    const sections = JSON.parse(body.rawContent) as { fullName: string; skills: string[] };
    expect(sections.fullName).toBe('Jane Doe');
    expect(sections.skills).toEqual(['Angular']);

    req.flush({
      id: 'new-1',
      userId: 'default-user',
      title: 'Jane Doe',
      rawContent: body.rawContent,
      parsedSectionsJson: '',
      createdAt: '2026-01-01T00:00:00',
      updatedAt: '2026-01-01T00:00:00',
    });

    expect(routerSpy.navigate).toHaveBeenCalledWith(['/resumes']);
    expect(snackBarSpy.open).toHaveBeenCalledWith('Resume created!', jasmine.anything(), jasmine.anything());
  });

  it('should update the uploaded resume via PATCH when saving a parsed resume', () => {
    component.onResumeParsed(parsedResult);
    component.fullName.setValue('Jane Doe Updated');

    component.saveResume();

    const req = httpMock.expectOne(`${api}/resumes/parsed-resume-id`);
    expect(req.request.method).toBe('PATCH');
    const body = req.request.body as { title: string; parsedSectionsJson: string };
    expect(body.title).toBe('Jane Doe Updated');
    expect(JSON.parse(body.parsedSectionsJson).fullName).toBe('Jane Doe Updated');

    req.flush({
      id: 'parsed-resume-id',
      userId: 'default-user',
      title: 'Jane Doe Updated',
      rawContent: 'full resume text',
      parsedSectionsJson: body.parsedSectionsJson,
      createdAt: '2026-01-01T00:00:00',
      updatedAt: '2026-01-01T00:00:00',
    });

    expect(snackBarSpy.open).toHaveBeenCalledWith('Resume updated!', jasmine.anything(), jasmine.anything());
  });

  it('should show a login prompt when the backend returns 401', () => {
    component.showUpload = false;
    component.fullName.setValue('Jane Doe');
    component.email.setValue('jane@example.com');

    component.saveResume();

    const req = httpMock.expectOne(`${api}/resumes`);
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

    expect(snackBarSpy.open).toHaveBeenCalledWith(
      'Please log in to save your resume.',
      jasmine.anything(),
      jasmine.anything()
    );
    expect(component.saving).toBeFalse();
  });

  it('should load and apply an existing resume in edit mode', () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [ResumeBuilderComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MatSnackBar, useValue: snackBarSpy },
        { provide: Router, useValue: routerSpy },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => 'existing-id' } } },
        },
      ],
    });

    const editFixture = TestBed.createComponent(ResumeBuilderComponent);
    const editComponent = editFixture.componentInstance;
    const editHttpMock = TestBed.inject(HttpTestingController);
    editFixture.detectChanges();

    expect(editComponent.isEditing).toBeTrue();
    expect(editComponent.showUpload).toBeFalse();

    const req = editHttpMock.expectOne(`${api}/resumes/existing-id`);
    expect(req.request.method).toBe('GET');
    req.flush({
      id: 'existing-id',
      userId: 'default-user',
      title: 'Existing',
      rawContent: '',
      parsedSectionsJson: JSON.stringify({
        fullName: 'Existing User',
        email: 'existing@example.com',
        phone: '',
        linkedin: '',
        portfolio: '',
        experiences: [],
        educations: [],
        skills: [],
      }),
      createdAt: '2026-01-01T00:00:00',
      updatedAt: '2026-01-01T00:00:00',
    });

    expect(editComponent.fullName.value).toBe('Existing User');
    expect(editComponent.email.value).toBe('existing@example.com');
    editHttpMock.verify();
  });
});
