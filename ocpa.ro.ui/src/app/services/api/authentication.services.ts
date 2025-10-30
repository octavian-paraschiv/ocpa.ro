import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { UntilDestroy } from '@ngneat/until-destroy';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { UserSessionInformation } from 'src/app/models/models-local';
import { UserType, AuthenticationResponse } from 'src/app/models/models-swagger';
import { UserTypeService } from 'src/app/services/api/user-type.service';
import { FingerprintService } from 'src/app/services/fingerprint.service';
import { SessionInformationService } from 'src/app/services/session-information.service';
import { environment } from 'src/environments/environment';


@Injectable()
@UntilDestroy()
export class AuthenticationService {
    private apiUserType: UserType;

    userLoginState$ = new BehaviorSubject<boolean>(false);
    shouldSendOtp$ = new BehaviorSubject<boolean>(false);

    constructor(
        private readonly sessionInfo: SessionInformationService,
        private readonly fingerprintService: FingerprintService,
        private readonly userTypeService: UserTypeService,
        private readonly http: HttpClient,
        private readonly router: Router) {

        if (this.isSessionExpired())
            this.sessionInfo.clearUserSessionInformation();

        this.userLoginState$.next(this.isUserLoggedIn());
    }

    logout(navigateHome: boolean) {
        this.sessionInfo.clearUserSessionInformation();
        this.userLoginState$.next(false);
        if (navigateHome) 
            this.router.navigate(['/meteo']);
    }

    generateOtp(): Observable<boolean> {
        const loginId = this.sessionInfo.getUserSessionInformation()?.loginId;

        return this.http.post<boolean>(
            `${environment.apiUrl}/users/generate-otp?loginId=${loginId}`, 
            undefined);
    }

    validateOtp(otp: string): Observable<string> {
        const loginId = this.sessionInfo.getUserSessionInformation()?.loginId;
        const hash = environment.ext.calc(loginId, otp, environment.ext.seed())
        const formParams = new HttpParams()
            .set('loginId', loginId)
            .set('password', hash);

        const headers = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });

        return this.http.post<AuthenticationResponse>(
            `${environment.apiUrl}/users/validate-otp`, 
            formParams, { headers } )
            .pipe(map(rsp => {
                const authResult = this.validateAuthenticationResponse(rsp, loginId);
                return (authResult?.length > 0) ? `auth.${authResult}` : undefined
            }));
    }

    authenticate(username: string, password: string, refreshAuth: boolean): Observable<string> {
        this.shouldSendOtp$.next(false);

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

        return this.http.post<AuthenticationResponse>(
            `${environment.apiUrl}/users/authenticate`, 
            formParams, { headers } )
            .pipe(map(rsp => {
                const authResult = this.validateAuthenticationResponse(rsp, username);
                return (authResult?.length > 0) ? `auth.${authResult}` : undefined
            }));
    }

    private validateAuthenticationResponse(rsp: AuthenticationResponse, username: string): string  {
        if (rsp?.loginId?.toUpperCase() !== username?.toUpperCase())
            return 'login-failed-1';
        
        if (!this.apiUserType)
            this.apiUserType = this.userTypeService.userType('API');

        if (rsp?.type === this.apiUserType?.id)
            return 'login-failed-3';

        const tokenExpiration = new Date();
        tokenExpiration.setSeconds(tokenExpiration.getSeconds() + rsp.validity);

        // Let the session cookie expire 10 seconds AFTER the token
        // NB: the session cookie expiration must be given in days
        const sessionInfoExpiration = (10 + rsp.validity) / (60 * 60 * 24);

        this.sessionInfo.setUserSessionInformation({
            loginId: rsp.loginId,
            token: rsp.token,
            anonymizedEmail: rsp.anonymizedEmail,
            tokenExpiration
        } as UserSessionInformation);
        
        if (rsp.sendOTP) {
            this.shouldSendOtp$.next(true);
            return 'sendOTP';
        }

        if (!(rsp?.validity > 0))
            return 'login-failed-2'

        this.userLoginState$.next(true);

        return undefined;
    }

    isSessionExpired(): boolean {
        const userSessionInfo = this.sessionInfo.getUserSessionInformation();
        if (userSessionInfo?.token?.length > 0 &&
            userSessionInfo.tokenExpiration) {
            const now = new Date();
            return new Date(userSessionInfo.tokenExpiration) < now;
        }
        
        return true;
    }

    get currentLoginId(): string {
        if (!this.isSessionExpired()) {
            return this.sessionInfo.getUserSessionInformation().loginId;
        }
        return undefined;
    }

    isSessionExpirationPending(): boolean {
        const userSessionInfo = this.sessionInfo.getUserSessionInformation();
        if (userSessionInfo?.token?.length > 0 &&
            userSessionInfo.tokenExpiration) {
            const immediatePast = new Date();
            immediatePast.setSeconds(immediatePast.getSeconds() + 10);
            return new Date(userSessionInfo.tokenExpiration) < immediatePast;
        }
        
        return false;
    }

    isUserLoggedIn(): boolean {
        const loggedInUser = this.sessionInfo.getUserSessionInformation()?.loginId;
        return (loggedInUser?.length > 0 && !this.isSessionExpired()/* && this.userLoginState$.getValue()*/);
    }

    public refreshAuthentication(): Observable<string> {
        if (this.isSessionExpirationPending()) {
            const loginId = this.sessionInfo.getUserSessionInformation()?.loginId;
            return this.http.post<AuthenticationResponse>(
                `${environment.apiUrl}/users/refresh-token?loginId=${loginId}`, undefined)
                .pipe(map(rsp => {
                    const authResult = this.validateAuthenticationResponse(rsp, loginId);
                    return (authResult?.length > 0) ? `auth.${authResult}` : undefined
                }));
        }

        return of(undefined);
    }
}