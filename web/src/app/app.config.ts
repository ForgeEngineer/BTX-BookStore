import { provideHttpClient } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import Aura from '@primeuix/themes/aura';
import { providePrimeNG } from 'primeng/config';
import { BookRepository } from './domain/book.repository';
import { CatalogRepository } from './domain/catalog.repository';
import { HttpBookRepository } from './infrastructure/http-book.repository';
import { HttpCatalogRepository } from './infrastructure/http-catalog.repository';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(),
    providePrimeNG({
      theme: {
        preset: Aura,
      },
    }),
    { provide: BookRepository, useClass: HttpBookRepository },
    { provide: CatalogRepository, useClass: HttpCatalogRepository },
  ],
};
