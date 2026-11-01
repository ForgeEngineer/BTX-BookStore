import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { Book, BookQuery, BookUpdate, NewBook, PagedResult } from '../domain/book';
import { BookRepository } from '../domain/book.repository';
import { API_URL } from './api-url';

const toError = (response: HttpErrorResponse) =>
  throwError(
    () => new Error(response.error?.detail || response.error?.title || 'Something went wrong.'),
  );

@Injectable()
export class HttpBookRepository implements BookRepository {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_URL}/books`;

  getBooks(query: BookQuery): Observable<PagedResult<Book>> {
    let params = new HttpParams()
      .set('page', query.page)
      .set('pageSize', query.pageSize)
      .set('sortField', query.sortField)
      .set('sortDesc', query.sortDesc);

    if (query.genre) {
      params = params.set('genre', query.genre);
    }

    return this.http.get<PagedResult<Book>>(this.url, { params }).pipe(catchError(toError));
  }

  getBook(id: string): Observable<Book> {
    return this.http.get<Book>(`${this.url}/${id}`).pipe(catchError(toError));
  }

  add(book: NewBook): Observable<Book> {
    return this.http.post<Book>(this.url, book).pipe(catchError(toError));
  }

  update(id: string, book: BookUpdate): Observable<Book> {
    return this.http.put<Book>(`${this.url}/${id}`, book).pipe(catchError(toError));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`).pipe(catchError(toError));
  }
}
