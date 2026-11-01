import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import Aura from '@primeuix/themes/aura';
import { providePrimeNG } from 'primeng/config';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    providePrimeNG({
      theme: {
        preset: Aura,
      },
    }),
  ],
};
