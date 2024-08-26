import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Router } from '@angular/router';
import { UserTypeService } from 'src/app/services/user-type.service';
import { AuthenticateResponse, UserType } from 'src/app/models/models-swagger';

@Injectable()
export class AuthenticationService {
    private currentAuthUser: AuthenticateResponse;
    private readonly userKey = 'ocpa_ro_admin_user';
    private readonly timeKey = 'ocpa_ro_admin_time';

    constructor(
        private readonly userTypeService: UserTypeService,
        private readonly http: HttpClient,
        private readonly router: Router) {
        this.currentAuthUser = JSON.parse(localStorage.getItem(this.userKey));
    }

    get currentUser() {
        return this.currentAuthUser;
    }

    get userToken() {
        return this.currentAuthUser?.token;
    }
    
    get validAdminUser() {
        const adminUserType = this.userTypeService.userType("ADM");
        return this.currentAuthUser?.token?.length > 0 &&
            this.currentAuthUser?.type === adminUserType?.id;
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
        const hash = environment.ext.calc(username, password, environment.ext.seed())
        const formParams = new HttpParams()
            .set('loginId', username)
            .set('password', hash);

        const headers = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });
        return this.http.post<AuthenticateResponse>(
            `${environment.apiUrl}/users/authenticate`, 
            formParams, { headers, withCredentials: true } )
            .pipe(map(rsp => this.validateAuthenticationResponse(rsp, username, reqUserType)));
    }

    validateAuthenticationResponse(rsp: AuthenticateResponse, username: string, reqUserType: UserType): string  {
        if (rsp?.loginId?.toUpperCase() !== username?.toUpperCase())
            return `<b>${username}</b> does not have access to this page.`;
        if (rsp?.type !== reqUserType?.id)
            return `This page requires <b>${reqUserType?.description}</b> user role and <b>${username}</b> does not have this role.`;
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