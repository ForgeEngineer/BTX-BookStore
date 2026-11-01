import { Component, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Select } from 'primeng/select';
import { BookStore } from '../../application/book-store';
import { GENRES, Genre } from '../../domain/book';

const ALL = 'All';

@Component({
  selector: 'app-header',
  imports: [FormsModule, Select],
  template: `
    <header class="flex items-center justify-between p-4 border-b border-surface-200">
      <h1 class="text-2xl">Book Store</h1>
      <p-select
        [options]="options"
        [ngModel]="selected()"
        (ngModelChange)="onGenreChange($event)"
        ariaLabel="Genre"
      />
    </header>
  `,
})
export class Header {
  private readonly store = inject(BookStore);

  protected readonly options: string[] = [ALL, ...GENRES];
  protected readonly selected = computed(() => this.store.genre() ?? ALL);

  protected onGenreChange(value: string): void {
    this.store.setGenre(value === ALL ? null : (value as Genre));
  }
}
