import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class Health {
  private readonly http = inject(HttpClient);

  check(): Observable<string> {
    return this.http.get('/api/health', { responseType: 'text' });
  }
}
