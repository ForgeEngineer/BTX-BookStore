import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { CatalogBook } from '../domain/book';
import { CatalogRepository } from '../domain/catalog.repository';
import { API_URL } from './api-url';

const toError = (response: HttpErrorResponse) =>
  throwError(
    () => new Error(response.error?.detail || response.error?.title || 'Something went wrong.'),
  );

@Injectable()
export class HttpCatalogRepository implements CatalogRepository {
  private readonly http = inject(HttpClient);

  search(q: string): Observable<CatalogBook[]> {
    return this.http
      .get<CatalogBook[]>(`${API_URL}/catalog/search`, { params: { q } })
      .pipe(catchError(toError));
  }
}
