import { Observable } from 'rxjs';
import { Book, BookQuery, BookUpdate, NewBook, PagedResult } from './book';

export abstract class BookRepository {
  abstract getBooks(query: BookQuery): Observable<PagedResult<Book>>;
  abstract getBook(id: string): Observable<Book>;
  abstract add(book: NewBook): Observable<Book>;
  abstract update(id: string, book: BookUpdate): Observable<Book>;
  abstract delete(id: string): Observable<void>;
}
