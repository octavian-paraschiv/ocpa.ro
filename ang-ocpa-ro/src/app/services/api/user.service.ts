import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { User } from 'src/app/models/models-swagger';

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
        return this.http.post(`${environment.apiUrl}/users/delete/${loginId}`, undefined);
    }
}