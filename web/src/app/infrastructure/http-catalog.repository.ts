import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CatalogBook } from '../domain/book';
import { CatalogRepository } from '../domain/catalog.repository';
import { API_URL } from './api-url';

@Injectable()
export class HttpCatalogRepository implements CatalogRepository {
  private readonly http = inject(HttpClient);

  search(q: string): Observable<CatalogBook[]> {
    return this.http.get<CatalogBook[]>(`${API_URL}/catalog/search`, { params: { q } });
  }
}
