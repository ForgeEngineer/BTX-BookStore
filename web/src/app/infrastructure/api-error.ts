import { HttpErrorResponse } from '@angular/common/http';
import { throwError } from 'rxjs';

export const toError = (response: HttpErrorResponse) =>
  throwError(
    () => new Error(response.error?.detail || response.error?.title || 'Something went wrong.'),
  );
