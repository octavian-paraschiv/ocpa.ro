import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthenticationService } from '../services/api/authentication.services';
import { environment } from 'src/environments/environment';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
    constructor(private authenticationService: AuthenticationService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        // add auth header with jwt if user is logged in and request is to the api url
        const loggedInUser = this.authenticationService.authUserChanged$.getValue();
        const token = loggedInUser?.token;
        const isLoggedIn = token?.length > 0;
        const isApiUrl = request.url.startsWith(environment.apiUrl);
        
        if (isLoggedIn && isApiUrl) {
            request = request.clone({
                setHeaders: {
                    Authorization: `Bearer ${token}`
                }
            });
        }
        
        if (!environment.production) {
            let headers = request.headers.set('Access-Control-Allow-Origin', '*');
            headers = headers.set('Access-Control-Allow-Methods', 'GET,POST');
            headers = headers.set('Access-Control-Allow-Headers', '*');
            request = request.clone( { headers });
        }

        return next.handle(request);
    }
}