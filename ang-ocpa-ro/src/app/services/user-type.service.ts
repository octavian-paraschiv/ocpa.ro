import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { UserType } from 'src/app/models/models-swagger';
import { environment } from 'src/environments/environment';

@UntilDestroy()
@Injectable()
export class UserTypeService {

    private _userTypes: UserType[] = [];

    constructor(private readonly httpClient: HttpClient) {
    }

    public init(): Observable<boolean> {
        console.debug('Calling UserTypeService.init...');
        return this.getUserTypes().pipe(
            map(userTypes => {
                this._userTypes = userTypes;
                return true;
            }),
            catchError(err => {
                console.error(err);
                this._userTypes = [];
                return of(false);
            })
        )
    }
    
    private getUserTypes(): Observable<UserType[]> {
        return this.httpClient.get<UserType[]>(`${environment.apiUrl}/user-types/all`);
    }

    public get userTypes() {
        return this._userTypes;
    }

    public userType(type: string) {
        return this._userTypes?.find(ut => ut?.code === type);
    }
}
