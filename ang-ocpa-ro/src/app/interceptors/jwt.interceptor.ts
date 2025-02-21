import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SessionInformationService } from 'src/app/services/session-information.service';
import { environment } from 'src/environments/environment';


@Injectable()
export class JwtInterceptor implements HttpInterceptor {
    constructor(
        private readonly sessionInfo: SessionInformationService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const token = this.sessionInfo.getUserSessionInformation()?.token;        
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