import { TestBed } from '@angular/core/testing';
import { Health } from './health';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';


describe('Health', () => {
  let service: Health;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(Health);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
