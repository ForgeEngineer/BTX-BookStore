import { Observable } from 'rxjs';
import { CatalogBook } from './book';

export abstract class CatalogRepository {
  abstract search(q: string): Observable<CatalogBook[]>;
}
