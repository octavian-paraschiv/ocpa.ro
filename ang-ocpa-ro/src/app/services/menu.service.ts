import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { combineLatest, Observable } from 'rxjs';
import { Menu, MenuDisplayMode, Menus } from 'src/app/models/models-swagger';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { map } from 'rxjs/operators';
import { Helper } from 'src/app/services/helper';
import { AuthenticationService } from 'src/app/services/authentication.services';

@UntilDestroy()
@Injectable()
export class MenuService {
    private _menus: Menus = {};

    constructor(private http: HttpClient,
        private authService: AuthenticationService) { 
    }

    get menus () { return this._menus; }

    init(): Observable<boolean> {
        return this.getMenus().pipe(
            untilDestroyed(this),
            map(menus => {
                const isUserLoggedIn = this.authService.isUserLoggedIn();

                this._menus.publicMenus = (menus?.publicMenus ?? [])
                .concat({
                    url: '/login',
                    name: 'Login',
                    menuIcon: 'faRightToBracket',
                    displayMode: MenuDisplayMode.HideOnMobile
                } as Menu)
                .filter(m => MenuService.showMenu(m));

                this._menus.appMenus = (menus?.appMenus ?? [])
                .concat({
                    url: '/logout',
                    name: 'Logout',
                    menuIcon: 'faRightFromBracket',
                    displayMode: MenuDisplayMode.HideOnMobile
                } as Menu)
                .filter(m => MenuService.showMenu(m));

                return isUserLoggedIn ? 
                    (this._menus?.appMenus?.length > 1) : 
                    (this._menus?.publicMenus?.length > 1);
            })
        );
    }

    private getMenus(): Observable<Menus> {
        const url = `${environment.apiUrl}/users/menus`;
        return this.http.get<Menus>(url);
    }

    private static showMenu(m: Menu): boolean {
        switch(m?.displayMode) {
            case MenuDisplayMode.AlwaysShow:
                return true;
            case MenuDisplayMode.HideOnMobile:
                return !Helper.isMobile();
            case MenuDisplayMode.ShowOnMobile:
                return Helper.isMobile();
            case MenuDisplayMode.AlwaysHide:
            default:
                return false;
        }
    }
}