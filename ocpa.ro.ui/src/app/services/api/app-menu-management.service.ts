import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "src/environments/environment";
import { UntilDestroy } from "@ngneat/until-destroy";
import { Application, ApplicationMenu, ApplicationUser, Menu } from 'src/app/models/swagger/access-management';

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

    public getAllAppMenus(): Observable<ApplicationMenu[]> {
        return this.http.get<ApplicationMenu[]>(`${environment.apiUrl}/applications/menus`);
    }

    public saveAppMenu(appId: number, menuId: number): Observable<ApplicationMenu> {
        return this.http.post<ApplicationMenu>(`${environment.apiUrl}/applications/${appId}/menu/save/${menuId}`, undefined);
    }

    public deleteAppMenu(appId: number, menuId: number): Observable<Object> {
        return this.http.post(`${environment.apiUrl}/applications/${appId}/menu/delete/${menuId}`, undefined);
    }

    //------------------------

    public getAppsForUser(userId: number): Observable<ApplicationUser[]> {
        return this.http.get<ApplicationUser[]>(`${environment.apiUrl}/users/${userId}/apps`);
    }

    public saveAppsForUser(userId: number, appsForUser: ApplicationUser[]): Observable<Object> {
        return this.http.post(`${environment.apiUrl}/users/${userId}/apps/save`, appsForUser ?? []);
    }
}       