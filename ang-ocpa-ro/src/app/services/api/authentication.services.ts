import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Router } from '@angular/router';
import { UserTypeService } from 'src/app/services/api/user-type.service';
import { AuthenticateResponse, UserType } from 'src/app/models/models-swagger';
import { FingerprintService } from 'src/app/services/fingerprint.service';

@Injectable()
export class AuthenticationService {
    private apiUserType: UserType;
    private readonly userKey = 'ocpa_ro_admin_user';
    private readonly passKey = 'ocpa_ro_admin_pass';
    private readonly timeKey = 'ocpa_ro_admin_time';

    authUserChanged$ = new BehaviorSubject<AuthenticateResponse>(undefined);

    constructor(
        private readonly fingerprintService: FingerprintService,
        private readonly userTypeService: UserTypeService,
        private readonly http: HttpClient,
        private readonly router: Router) {

        let authUser: AuthenticateResponse = undefined;
        
        try {
            authUser = JSON.parse(localStorage.getItem(this.userKey)) as AuthenticateResponse;
        } catch {
            authUser = undefined;
        }

        if (this.isSessionExpired()) {
            authUser = undefined;
            localStorage.removeItem(this.passKey);
            localStorage.removeItem(this.userKey);
            localStorage.removeItem(this.timeKey);
        }

        this.authUserChanged$.next(authUser);
    }

    logout(navigateHome: boolean) {
        try {
            // remove user from local storage to log user out
            localStorage.removeItem(this.passKey);
            localStorage.removeItem(this.userKey);
            localStorage.removeItem(this.timeKey);
        } catch {
        }

        this.authUserChanged$.next(undefined);

        if (navigateHome) 
            this.router.navigate(['/meteo']);
    }

    authenticate(username: string, password: string): Observable<string> {
        const hash = environment.ext.calc(username, password, environment.ext.seed())
        const formParams = new HttpParams()
            .set('loginId', username)
            .set('password', hash);

        let headers = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });
        const fingerprint = this.fingerprintService.Fingerprint;
        if (fingerprint?.length > 0)
            headers = headers.set('X-Device-Id', fingerprint);

        return this.http.post<AuthenticateResponse>(
            `${environment.apiUrl}/users/authenticate`, 
            formParams, { headers, withCredentials: true } )
            .pipe(map(rsp => {
                const authResult = this.validateAuthenticationResponse(rsp, username, password);
                return (authResult?.length > 0) ? `auth.${authResult}` : undefined
            }));
    }

    private validateAuthenticationResponse(rsp: AuthenticateResponse, username: string, password: string): string  {
        if (rsp?.loginId?.toUpperCase() !== username?.toUpperCase())
            return 'login-failed-1';
        
        if (!(rsp?.validity > 0))
            return 'login-failed-2'

        if (!this.apiUserType)
            this.apiUserType = this.userTypeService.userType('API');

        if (rsp?.type === this.apiUserType?.id)
            return 'login-failed-3';
        
        // store user details and jwt token in local storage to keep user logged in between page refreshes
        localStorage.setItem(this.userKey, JSON.stringify(rsp));
        localStorage.setItem(this.passKey, password);
        
        const loggedInTime = new Date();
        loggedInTime.setSeconds(loggedInTime.getSeconds() + rsp.validity);
        localStorage.setItem(this.timeKey, JSON.stringify(loggedInTime.getTime()));

        this.authUserChanged$.next(rsp);

        return undefined;
    }

    isSessionExpired(): boolean {
        const loggedInTime = localStorage.getItem(this.timeKey);
        if (loggedInTime === null) {
          return false;
        }
    
        return Number(loggedInTime) < new Date(new Date().toUTCString()).getTime();
    }

    isUserLoggedIn(): boolean {
        const loggedInUser = this.authUserChanged$.getValue();
        return (loggedInUser?.token?.length > 0 && !this.isSessionExpired())
    }

    refreshAuthentication(): Observable<string> {
        if (this.isSessionExpired()) {
            const user = JSON.parse(localStorage.getItem(this.userKey)) as AuthenticateResponse;
            const pass = localStorage.getItem(this.passKey);
            return this.authenticate(user?.loginId, pass);
        }
        
        return of(undefined);
    }
}