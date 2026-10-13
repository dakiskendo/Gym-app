import { TestBed } from '@angular/core/testing';
import { App } from './app';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

describe('App', () => {
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    httpTesting.expectOne('/api/health');
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should show Healthy when the API answers 200', async () => {
    const fixture = TestBed.createComponent(App);

    const req = httpTesting.expectOne('/api/health');
    req.flush('Healthy');
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(text).toContain('API status: Healthy');
  });

  it('should show unreachable when the API answers 503', async () => {
    const fixture = TestBed.createComponent(App);

    const req = httpTesting.expectOne('/api/health');
    req.flush('Unhealthy', { status: 503, statusText: 'Service Unavailable' });
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(text).toContain('API status: API or database unreachable');
  });
});
