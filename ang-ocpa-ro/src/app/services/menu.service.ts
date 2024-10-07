import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { combineLatest, Observable } from 'rxjs';
import { Menu, MenuDisplayMode } from 'src/app/models/models-swagger';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { map } from 'rxjs/operators';
import { Helper } from 'src/app/services/helper';

export interface MenuSpec {
    publicMenus: Menu[];
    appMenus: Menu[];
}

@UntilDestroy()
@Injectable()
export class MenuService {
    private _menus: MenuSpec = {
        appMenus: [],
        publicMenus: []
    }

    constructor(private http: HttpClient) { 
    }

    get menus () { return this._menus; }

    init(): Observable<boolean> {
        const oa = this.getMenus(true).pipe(
            untilDestroyed(this),
            map(menus => {
                this._menus.appMenus = menus
                    .filter(m => MenuService.showMenu(m))
                    .map(m => MenuService.DecodeIcons(m))
                    .concat(({
                        url: '/logout',
                        name: 'Logout',
                        smallIcon: 'faRightFromBracket'
                    }) as Menu);                    

                return (this._menus?.appMenus?.length > 1);
            })
        );

        const op = this.getMenus(false).pipe(
            untilDestroyed(this),
            map(menus => {
                this._menus.publicMenus = menus
                    .filter(m => MenuService.showMenu(m))
                    .map(m => MenuService.DecodeIcons(m))
                    .concat(({
                        url: '/login',
                        name: 'Login',
                        smallIcon: 'faRightToBracket'
                    }) as Menu);

                return (this._menus?.publicMenus?.length > 1);
            })
        );

        return combineLatest([oa, op]).pipe(
            untilDestroyed(this),
            map(([value1, value2]) => value1 && value2)
          );
    }

    private getMenus(appMenus: boolean): Observable<Menu[]> {
        const url = `${environment.apiUrl}/users/${ appMenus ? 'app-menus' : 'menus' }`
        return this.http.get<Menu[]>(url);
    }

    private static DecodeIcons(m: Menu): Menu {
        return ({
            code: m.code,
            id: m.id,
            name: m.name,
            url: m.url,
            largeIcon: m.largeIcon?.length > 0 ? window.atob(m.largeIcon) : null,
            smallIcon: m.smallIcon?.length > 0 ? window.atob(m.smallIcon) : null,
        } as Menu);
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