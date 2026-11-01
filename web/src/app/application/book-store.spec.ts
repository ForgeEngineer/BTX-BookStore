import { TestBed } from '@angular/core/testing';
import { Observable, of, throwError } from 'rxjs';
import { Book, BookQuery, PagedResult } from '../domain/book';
import { BookRepository } from '../domain/book.repository';
import { BookStore } from './book-store';

const book: Book = {
  id: '1',
  title: 'Dune',
  author: 'Frank Herbert',
  isbn: '9780441013593',
  publishedYear: 1965,
  coverUrl: null,
  openLibraryKey: '/works/OL893415W',
  description: null,
  genre: 'SciFi',
  price: 10,
  stock: 5,
  createdAtUtc: '2026-01-01T00:00:00Z',
};

class FakeBookRepository extends BookRepository {
  lastQuery?: BookQuery;
  result: Observable<PagedResult<Book>> = of({
    items: [book],
    page: 1,
    pageSize: 10,
    totalCount: 1,
  });

  getBooks(query: BookQuery): Observable<PagedResult<Book>> {
    this.lastQuery = query;
    return this.result;
  }

  getBook(): Observable<Book> {
    return of(book);
  }

  add(): Observable<Book> {
    return of(book);
  }

  update(): Observable<Book> {
    return of(book);
  }

  delete(): Observable<void> {
    return of(undefined);
  }
}

describe('BookStore', () => {
  let repository: FakeBookRepository;
  let store: BookStore;

  beforeEach(() => {
    repository = new FakeBookRepository();
    TestBed.configureTestingModule({
      providers: [{ provide: BookRepository, useValue: repository }],
    });
    store = TestBed.inject(BookStore);
  });

  it('setGenre sets the genre, resets page to 1 and loads books', () => {
    store.page.set(3);

    store.setGenre('SciFi');

    expect(store.genre()).toBe('SciFi');
    expect(store.page()).toBe(1);
    expect(repository.lastQuery?.genre).toBe('SciFi');
    expect(repository.lastQuery?.page).toBe(1);
    expect(store.books()).toEqual([book]);
    expect(store.totalCount()).toBe(1);
    expect(store.loading()).toBe(false);
  });

  it('load sets error when the repository fails', () => {
    repository.result = throwError(() => new Error('Could not load books.'));

    store.load();

    expect(store.error()).toBe('Could not load books.');
    expect(store.loading()).toBe(false);
  });
});
