import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Health } from './core/health';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly health = inject(Health);
  protected readonly status = signal('checking...');

  constructor() {
    this.health.check().subscribe({
      next: result => this.status.set(result),
      error: () => this.status.set('API or database unreachable'),
    });
  }
}
