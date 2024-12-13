import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { Menu, EMenuDisplayMode, Menus, VMenu } from 'src/app/models/models-swagger';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { map } from 'rxjs/operators';
import { Helper } from 'src/app/services/helper';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { FingerprintService } from 'src/app/services/fingerprint.service';

@UntilDestroy()
@Injectable()
export class MenuService {
    private _menus: Menus = {};

    constructor(private readonly http: HttpClient,
        private readonly fingerprintService: FingerprintService,
        private readonly authService: AuthenticationService) { 
    }

    get menus () { return this._menus; }

    init(): Observable<boolean> {
        const fingerprint = this.fingerprintService.Fingerprint;
        return this.getMenus().pipe(
            untilDestroyed(this),
            map(menus => {
                const isUserLoggedIn = this.authService.isUserLoggedIn();
                let publicMenus = (menus?.publicMenus ?? []);
                if (fingerprint === menus?.deviceId) {
                    publicMenus = publicMenus .concat({
                        url: '/login',
                        name: 'Login',
                        code: 'LIN',
                        menuIcon: 'faRightToBracket',
                        displayMode: EMenuDisplayMode.HideOnMobile
                    } as Menu);
                }
                this._menus.publicMenus = publicMenus.filter(m => MenuService.showMenu(m));

                this._menus.appMenus = (menus?.appMenus ?? [])
                .concat({
                    url: '/logout',
                    name: 'Logout',
                    code: 'LOUT',
                    menuIcon: 'faRightFromBracket',
                    displayMode: EMenuDisplayMode.HideOnMobile
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
}