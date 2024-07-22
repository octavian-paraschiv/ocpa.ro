import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { User, UserType } from '../models/user';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UserService {

    constructor(private http: HttpClient) { }

    getAllUsers(): Observable<User[]> {
        return this.http.get<User[]>(`${environment.apiUrl}/users/all`);
    }

    saveUser(user: User): Observable<User> {
        return this.http.post<User>(`${environment.apiUrl}/users/save`, user);
    }

    deleteUser(loginId: string): Observable<Object> {
        return this.http.delete(`${environment.apiUrl}/users/delete/${loginId}`);
    }
}