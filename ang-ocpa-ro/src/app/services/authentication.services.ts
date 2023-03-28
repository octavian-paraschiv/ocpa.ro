import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { User } from '../models/user';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthenticationService {
    private currentUserSubject: BehaviorSubject<User>;
    public currentUser: Observable<User>;
    
    readonly userKey = 'ocpa_ro_admin_user';
    readonly timeKey = 'ocpa_ro_admin_time';

    constructor(private http: HttpClient) {
        this.currentUserSubject = new BehaviorSubject<User>(JSON.parse(localStorage.getItem(this.userKey)));
        this.currentUser = this.currentUserSubject.asObservable();
    }

    public get currentUserValue(): User {
        return this.currentUserSubject.value;
    }

    login(username: string, password: string) {
        const formParams = new HttpParams()
            .set('loginId', username)
            .set('password', password);

        const headers = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });

        return this.http.post<User>(
            `${environment.apiUrl}/users/authenticate`, 
            formParams, { headers, withCredentials: true } )
            .pipe(tap(user => this.setLoginUser(user)));
    }

    logout() {
        // remove user from local storage to log user out
        localStorage.removeItem(this.userKey);
        localStorage.removeItem(this.timeKey);
        this.currentUserSubject.next(null);
    }

    setLoginUser(loginUser: User, sessionDuration = 1)  {
        // store user details and jwt token in local storage to keep user logged in between page refreshes
        const loggedInTime = new Date();
        loggedInTime.setMinutes(loggedInTime.getMinutes() + sessionDuration);

        localStorage.setItem(this.userKey, JSON.stringify(loginUser));
        localStorage.setItem(this.timeKey, JSON.stringify(loggedInTime.getTime()));
        this.currentUserSubject.next(loginUser);
    }

    isSessionExpired(): boolean {
        const loggedInTime = localStorage.getItem(this.timeKey);
        if (loggedInTime === null) {
          return false;
        }
    
        return Number(loggedInTime) < new Date(new Date().toUTCString()).getTime();
      }
}