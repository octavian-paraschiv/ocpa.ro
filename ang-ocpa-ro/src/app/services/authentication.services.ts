import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Router } from '@angular/router';
import { UserTypeService } from 'src/app/services/user-type.service';
import { AuthenticateResponse, UserType } from 'src/app/models/models-swagger';

@Injectable()
export class AuthenticationService {
    private apiUserType: UserType;
    private readonly userKey = 'ocpa_ro_admin_user';
    private readonly timeKey = 'ocpa_ro_admin_time';

    authUserChanged$ = new BehaviorSubject<AuthenticateResponse>(undefined);

    constructor(
        private readonly userTypeService: UserTypeService,
        private readonly http: HttpClient,
        private readonly router: Router) {

        let authUser = JSON.parse(localStorage.getItem(this.userKey)) as AuthenticateResponse;

        if (this.isSessionExpired()) {
            authUser = undefined;
            localStorage.removeItem(this.userKey);
            localStorage.removeItem(this.timeKey);
        }

        this.authUserChanged$.next(authUser);
    }

    logout(navigateHome: boolean) {
        try {
            // remove user from local storage to log user out
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

        const headers = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });
        return this.http.post<AuthenticateResponse>(
            `${environment.apiUrl}/users/authenticate`, 
            formParams, { headers, withCredentials: true } )
            .pipe(map(rsp => this.validateAuthenticationResponse(rsp, username)));
    }

    validateAuthenticationResponse(rsp: AuthenticateResponse, username: string): string  {
        if (rsp?.loginId?.toUpperCase() !== username?.toUpperCase())
            return `User <b>${username}</b> cannot log in [1].`;
        
        if (!this.apiUserType)
            this.apiUserType = this.userTypeService.userType('API');

        if (rsp?.type === this.apiUserType?.id)
            return `User: <b>${username}</b> does not have any application roles.`;
        
        if (!(rsp?.validity > 0))
            return `User: <b>${username}</b> cannot log in [2]`;

        // store user details and jwt token in local storage to keep user logged in between page refreshes
        localStorage.setItem(this.userKey, JSON.stringify(rsp));
        
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
}