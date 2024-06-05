import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AuthenticationResponse, UserType } from '../models/user';
import { environment } from 'src/environments/environment';
import * as sha1 from 'sha1';
import { Router } from '@angular/router';

@Injectable()
export class AuthenticationService {
    private currentAuthUser: AuthenticationResponse;
    private readonly userKey = 'ocpa_ro_admin_user';
    private readonly timeKey = 'ocpa_ro_admin_time';

    constructor(private http: HttpClient,
        private router: Router) {
        this.currentAuthUser = JSON.parse(localStorage.getItem(this.userKey));
    }

    get userToken() {
        return this.currentAuthUser?.token;
    }
    
    get validAdminUser() {
        return this.currentAuthUser?.token?.length > 0 &&
            this.currentAuthUser?.type === UserType.Admin;
    }

    logout(navigateHome: boolean) {
        try {
            // remove user from local storage to log user out
            localStorage.removeItem(this.userKey);
            localStorage.removeItem(this.timeKey);
        } catch {
        }

        this.currentAuthUser = undefined;

        if (navigateHome) 
            this.router.navigate(['/']);
    }

    authenticate(username: string, password: string, reqUserType: UserType): Observable<string> {
        const formParams = new HttpParams()
            .set('loginId', username)
            .set('password', sha1(password));

            const headers = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });

        return this.http.post<AuthenticationResponse>(
            `${environment.apiUrl}/users/authenticate`, 
            formParams, { headers, withCredentials: true } )
            .pipe(map(rsp => this.validateAuthenticationResponse(rsp, username, reqUserType)));
    }

    validateAuthenticationResponse(rsp: AuthenticationResponse, username: string, reqUserType: UserType): string  {
        if (rsp?.loginId?.toUpperCase() !== username?.toUpperCase())
            return `<b>${username}</b> does not have access to this page.`;
        if (rsp?.type !== reqUserType)
            return `This page requires <b>${UserType[reqUserType]}</b> user role and <b>${username}</b> does not have this role.`;
        if (!(rsp?.validity > 0))
            return `<b>${username}</b> was not granted access to this page`;

        // store user details and jwt token in local storage to keep user logged in between page refreshes
        localStorage.setItem(this.userKey, JSON.stringify(rsp));
        
        const loggedInTime = new Date();
        loggedInTime.setSeconds(loggedInTime.getSeconds() + rsp.validity);
        localStorage.setItem(this.timeKey, JSON.stringify(loggedInTime.getTime()));

        this.currentAuthUser = rsp;

        return undefined;
    }

    isSessionExpired(): boolean {
        const loggedInTime = localStorage.getItem(this.timeKey);
        if (loggedInTime === null) {
          return false;
        }
    
        return Number(loggedInTime) < new Date(new Date().toUTCString()).getTime();
      }
}