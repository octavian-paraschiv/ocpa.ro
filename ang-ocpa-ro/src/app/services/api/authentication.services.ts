import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Router } from '@angular/router';
import { UserTypeService } from 'src/app/services/api/user-type.service';
import { AuthenticationResponse, UserType } from 'src/app/models/models-swagger';
import { FingerprintService } from 'src/app/services/fingerprint.service';
import { TranslateService } from '@ngx-translate/core';

@Injectable()
export class AuthenticationService {
    private apiUserType: UserType;
    private readonly userKey = 'ocpa_ro_admin_user';
    private readonly passKey = 'ocpa_ro_admin_pass';
    private readonly timeKey = 'ocpa_ro_admin_time';

    authUserChanged$ = new BehaviorSubject<AuthenticationResponse>(undefined);
    mfaUserChanged$ = new BehaviorSubject<string>(null);

    constructor(
        private readonly fingerprintService: FingerprintService,
        private readonly userTypeService: UserTypeService,
        private readonly http: HttpClient,
        private readonly router: Router) {

        let authUser: AuthenticationResponse = undefined;
        
        try {
            authUser = JSON.parse(localStorage.getItem(this.userKey)) as AuthenticationResponse;
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

    sendMfa(mfa: string) {
        const formParams = new HttpParams()
            .set('loginId', this.mfaUserChanged$.getValue())
            .set('password', mfa);

        let headers = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });

        const pass = localStorage.getItem(this.passKey);

        return this.http.post<AuthenticationResponse>(
            `${environment.apiUrl}/users/mfa`, 
            formParams, { headers, withCredentials: true } )
            .pipe(map(rsp => {
                const authResult = this.validateAuthenticationResponse(rsp, this.mfaUserChanged$.getValue(), pass);
                return (authResult?.length > 0) ? `auth.${authResult}` : undefined
            }));
    }

    authenticate(username: string, password: string, refreshAuth: boolean): Observable<string> {
        this.mfaUserChanged$.next(null);

        const hash = environment.ext.calc(username, password, environment.ext.seed())
        const formParams = new HttpParams()
            .set('loginId', username)
            .set('password', hash);

        let headers = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });

        const fingerprint = this.fingerprintService.Fingerprint;
        if (fingerprint?.length > 0)
            headers = headers.set('X-Device-Id', fingerprint);

        if (refreshAuth)
            headers = headers.set('X-Refresh-Auth', '1');

        headers = headers.set('X-Language', navigator.language ?? 'en');

        return this.http.post<AuthenticationResponse>(
            `${environment.apiUrl}/users/authenticate`, 
            formParams, { headers, withCredentials: true } )
            .pipe(map(rsp => {
                const authResult = this.validateAuthenticationResponse(rsp, username, password);
                return (authResult?.length > 0) ? `auth.${authResult}` : undefined
            }));
    }

    private validateAuthenticationResponse(rsp: AuthenticationResponse, username: string, password: string): string  {
        if (rsp?.loginId?.toUpperCase() !== username?.toUpperCase())
            return 'login-failed-1';
        
        if (!this.apiUserType)
            this.apiUserType = this.userTypeService.userType('API');

        if (rsp?.type === this.apiUserType?.id)
            return 'login-failed-3';
        
        localStorage.setItem(this.userKey, JSON.stringify(rsp));
        localStorage.setItem(this.passKey, password);
        
        if (rsp.useMFA) {
            this.mfaUserChanged$.next(rsp.loginId);
            return 'useMfa';
        }

        if (!(rsp?.validity > 0))
            return 'login-failed-2'

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
            const user = JSON.parse(localStorage.getItem(this.userKey)) as AuthenticationResponse;
            const pass = localStorage.getItem(this.passKey);
            return this.authenticate(user?.loginId, pass, true);
        }
        
        return of(undefined);
    }
}