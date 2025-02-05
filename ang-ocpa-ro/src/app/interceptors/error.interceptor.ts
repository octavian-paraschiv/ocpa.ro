import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthenticationService } from '../services/api/authentication.services';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
    constructor(private authenticationService: AuthenticationService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(request).pipe(catchError((err: HttpErrorResponse) => {
            const error = err?.error?.toString() ?? err?.message ?? err?.error?.message ?? err?.statusText;
            if ([401, 403].indexOf(err.status) !== -1) {
                this.authenticationService.logout(false);
            }
            return throwError(error ?? 'unspecified');
        }))
    }
}