import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "src/environments/environment";
import { UntilDestroy } from "@ngneat/until-destroy";
import { TranslateService } from '@ngx-translate/core';
import { Application, ApplicationMenu, ApplicationUser, Menu } from 'src/app/models/models-swagger';

@UntilDestroy()
@Injectable()
export class AppMenuManagementService {
    constructor(private readonly http: HttpClient) {
    }

    //------------------------
   
    public getAllApps(): Observable<Application[]> {
        return this.http.get<Application[]>(`${environment.apiUrl}/applications/all`);
    }

    public saveApp(app: Application): Observable<Application> {
        return this.http.post<Application>(`${environment.apiUrl}/applications/save`, app);
    }

    public deleteApp(appId: number): Observable<Object> {
        return this.http.post(`${environment.apiUrl}/applications/delete/${appId}`, undefined);
    }

    //------------------------

    public getAllMenus(): Observable<Menu[]> {
        return this.http.get<Menu[]>(`${environment.apiUrl}/menus/all`);
    }

    public saveMenu(menu: Menu): Observable<Menu> {
        return this.http.post<Menu>(`${environment.apiUrl}/menus/save`, menu);
    }

    public deleteMenu(menuId: number): Observable<Object> {
        return this.http.post(`${environment.apiUrl}/menus/delete/${menuId}`, undefined);
    }

    //------------------------

    public getAllAppMenus(appId: number): Observable<ApplicationMenu[]> {
        return this.http.get<ApplicationMenu[]>(`${environment.apiUrl}/applications/${appId}/menus`);
    }

    public saveAppMenu(appId: number, appMenu: ApplicationMenu): Observable<ApplicationMenu> {
        return this.http.post<ApplicationMenu>(`${environment.apiUrl}/applications/${appId}/menu/save`, appMenu);
    }

    public deleteAppMenu(appId: number, appMenuId: number): Observable<Object> {
        return this.http.post(`${environment.apiUrl}/applications/${appId}/menu/delete/${appMenuId}`, undefined);
    }

    //------------------------

    public getAllAppUsers(appId: number): Observable<ApplicationUser[]> {
        return this.http.get<ApplicationUser[]>(`${environment.apiUrl}/applications/${appId}/users`);
    }

    public saveAppUser(appId: number, appUser: ApplicationUser): Observable<ApplicationUser> {
        return this.http.post<ApplicationUser>(`${environment.apiUrl}/applications/${appId}/user/save`, appUser);
    }

    public deleteAppUser(appId: number, appUserId: number): Observable<Object> {
        return this.http.post(`${environment.apiUrl}/applications/${appId}/user/delete/${appUserId}`, undefined);
    }
}   