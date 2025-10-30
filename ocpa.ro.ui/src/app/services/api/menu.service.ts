import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { BehaviorSubject, Observable } from 'rxjs';
import { Menu, EMenuDisplayMode, Menus, VMenu } from 'src/app/models/models-swagger';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { map } from 'rxjs/operators';
import { Helper } from 'src/app/helpers/helper';
import { AuthenticationService } from 'src/app/services/api/authentication.services';
import { FingerprintService } from 'src/app/services/fingerprint.service';

export enum UrlKind {
    Public,
    App,
    Unavailable
}

@UntilDestroy()
@Injectable()
export class MenuService {
    private _menus: Menus = {};
    singleMenuApp$ = new BehaviorSubject<boolean>(true);

    constructor(private readonly http: HttpClient,
        private readonly fingerprintService: FingerprintService,
        private readonly authService: AuthenticationService) { 
    }

    get menus () { return this._menus; }

    getUrlKind(url: string): UrlKind {
        if (url === '/' || url === '/meteo') 
            return UrlKind.Public;

        url = (url ?? '').toUpperCase();

        return ((this._menus?.publicMenus ?? []).filter(m => url.startsWith(m.url.toUpperCase())).length > 0) ?
            UrlKind.Public :
            ((this._menus?.appMenus ?? []).filter(m => url.startsWith(m.url.toUpperCase())).length > 0) ?
                UrlKind.App :
                UrlKind.Unavailable;
    }

    init(): Observable<boolean> {
        const fingerprint = this.fingerprintService.Fingerprint;
        return this.getMenus().pipe(
            untilDestroyed(this),
            map(menus => {
                const isUserLoggedIn = this.authService.isUserLoggedIn();
                let publicMenus = (menus?.publicMenus ?? []);
                if (fingerprint === menus?.deviceId) {
                    publicMenus = publicMenus.concat({
                        url: '/login',
                        name: 'login',
                        menuIcon: 'faRightToBracket',
                        displayMode: EMenuDisplayMode.AlwaysShow
                    } as Menu);
                }
                this._menus.publicMenus = publicMenus.filter(m => MenuService.showMenu(m));

                this._menus.appMenus = (menus?.appMenus ?? [])
                .concat({
                    url: '/logout',
                    name: 'logout',
                    menuIcon: 'faRightFromBracket',
                    displayMode: EMenuDisplayMode.AlwaysShow
                } as Menu)
                .filter(m => MenuService.showMenu(m));

                const singleMenuApp = isUserLoggedIn && !((menus?.appMenus ?? []).length > 1);
                this.singleMenuApp$.next(singleMenuApp);

                return isUserLoggedIn ? 
                    (this._menus?.appMenus?.length > 1) : 
                    (this._menus?.publicMenus?.length > 1);
            })
        );
    }

    private getMenus(): Observable<Menus> {
        const url = `${environment.apiUrl}/users/menus`;
        const fingerprint = this.fingerprintService.Fingerprint;
        
        if (fingerprint?.length > 0) {
            const headers = new HttpHeaders({ 'X-Device-Id': fingerprint });
            return this.http.get<Menus>(url, { headers });

        }

        return this.http.get<Menus>(url);
    }

    private static showMenu(m: VMenu): boolean {
        switch(m?.displayMode) {
            case EMenuDisplayMode.AlwaysShow:
                return true;
            case EMenuDisplayMode.HideOnMobile:
                return !Helper.isMobile();
            case EMenuDisplayMode.ShowOnMobile:
                return Helper.isMobile();
            case EMenuDisplayMode.AlwaysHide:
            default:
                return false;
        }
    }

    public static isAuthMenu(m: Menu): boolean {
        switch(m?.url?.toLowerCase()) {
            case '/login':
            case '/logout':
                return true;
            default: 
                return false;
        }
    }
}