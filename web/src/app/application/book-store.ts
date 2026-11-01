import { Injectable, inject, signal } from '@angular/core';
import { Observable, Subscription, tap } from 'rxjs';
import { Book, BookUpdate, Genre, NewBook } from '../domain/book';
import { BookRepository } from '../domain/book.repository';

@Injectable({ providedIn: 'root' })
export class BookStore {
  private readonly repository = inject(BookRepository);
  private loadSubscription?: Subscription;

  readonly genre = signal<Genre | null>(null);
  readonly page = signal(1);
  readonly pageSize = signal(10);
  readonly sortField = signal('title');
  readonly sortDesc = signal(false);
  readonly books = signal<Book[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  load(): void {
    this.loadSubscription?.unsubscribe();
    this.loading.set(true);
    this.error.set(null);

    this.loadSubscription = this.repository
      .getBooks({
        genre: this.genre() ?? undefined,
        page: this.page(),
        pageSize: this.pageSize(),
        sortField: this.sortField(),
        sortDesc: this.sortDesc(),
      })
      .subscribe({
        next: (result) => {
          this.books.set(result.items);
          this.totalCount.set(result.totalCount);
          this.loading.set(false);
        },
        error: (error: Error) => {
          this.error.set(error.message);
          this.loading.set(false);
        },
      });
  }

  setGenre(genre: Genre | null): void {
    this.genre.set(genre);
    this.page.set(1);
    this.load();
  }

  setPage(page: number, pageSize: number): void {
    this.page.set(page);
    this.pageSize.set(pageSize);
    this.load();
  }

  setSort(field: string, desc: boolean): void {
    this.sortField.set(field);
    this.sortDesc.set(desc);
    this.load();
  }

  add(book: NewBook): Observable<Book> {
    return this.repository.add(book).pipe(tap(() => this.load()));
  }

  update(id: string, book: BookUpdate): Observable<Book> {
    return this.repository.update(id, book).pipe(tap(() => this.load()));
  }

  remove(id: string): Observable<void> {
    return this.repository.delete(id).pipe(
      tap(() => {
        if (this.books().length === 1 && this.page() > 1) {
          this.page.update((page) => page - 1);
        }
        this.load();
      }),
    );
  }
}
