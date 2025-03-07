import { Injectable, inject } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthenticationService } from '../services/api/authentication.services';
import { FailedAuthenticationResponse } from 'src/app/models/models-swagger';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
    private authService = inject(AuthenticationService);

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(request).pipe(catchError((err: HttpErrorResponse) => {
            const far = err.error as FailedAuthenticationResponse;
            if (far)
                return throwError(far);    

            if ([401, 403].indexOf(err.status) !== -1) {
                this.authService.logout(false);
            }

            const error = err?.error?.toString() ?? err?.message ?? err?.error?.message ?? err?.statusText;
            return throwError(error ?? 'unspecified');
        }))
    }
}