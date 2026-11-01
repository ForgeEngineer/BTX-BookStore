import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Book, BookQuery, BookUpdate, NewBook, PagedResult } from '../domain/book';
import { BookRepository } from '../domain/book.repository';
import { API_URL } from './api-url';

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

    return this.http.get<PagedResult<Book>>(this.url, { params });
  }

  getBook(id: string): Observable<Book> {
    return this.http.get<Book>(`${this.url}/${id}`);
  }

  add(book: NewBook): Observable<Book> {
    return this.http.post<Book>(this.url, book);
  }

  update(id: string, book: BookUpdate): Observable<Book> {
    return this.http.put<Book>(`${this.url}/${id}`, book);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
