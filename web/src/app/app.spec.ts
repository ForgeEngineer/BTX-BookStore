import { TestBed } from '@angular/core/testing';
import { ConfirmationService, MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { App } from './app';
import { BookRepository } from './domain/book.repository';
import { CatalogRepository } from './domain/catalog.repository';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        MessageService,
        ConfirmationService,
        { provide: CatalogRepository, useValue: {} },
        {
          provide: BookRepository,
          useValue: { getBooks: () => of({ items: [], page: 1, pageSize: 10, totalCount: 0 }) },
        },
      ],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the header, body and footer', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('app-header')?.textContent).toContain('Book Store');
    expect(compiled.querySelector('app-body')?.textContent).toContain('No books found.');
    expect(compiled.querySelector('app-footer')?.textContent).toContain('Technical Assessment');
  });
});
