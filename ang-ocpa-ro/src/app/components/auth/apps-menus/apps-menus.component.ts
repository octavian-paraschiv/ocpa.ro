import { Component, OnInit, NgZone } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { TranslateService } from '@ngx-translate/core';
import { BaseAuthComponent } from 'src/app/components/auth/base/BaseAuthComponent';
import { Application, ApplicationMenu, EMenuDisplayMode, Menu } from 'src/app/models/models-swagger';
import { AppMenuManagementService } from 'src/app/services/app-menu-management.service';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { faCheck, fas, faSquareMinus, faSquarePen, faSquarePlus } from '@fortawesome/free-solid-svg-icons';

@UntilDestroy()
@Component({
    selector: 'apps-menus',
    templateUrl: './apps-menus.component.html'
})
export class AppsMenusComponent extends BaseAuthComponent implements OnInit {
    icons = fas;
    faAdd = faSquarePlus;
    faEdit = faSquarePen;
    faRemove = faSquareMinus;
    faCheck = faCheck;
    size = "grow-6";

    menus: Menu[] = [];
    menusColumns = ['menu-add', 'menu-edit', 'menu-delete', 'menu-name', 'menu-url', 'menu-display-mode', 'menu-icon', 'menu-builtin', 'menu-filler'];

    apps: Application[] = [];
    appsColumns = ['app-add', 'app-edit', 'app-delete', 'app-name', 'app-code', 'app-login-required', 'app-admin-mode', 'app-builtin', 'app-filler'];

    appsMenus: { [id: number]: ApplicationMenu[] } = {};

    constructor(private readonly appMenuService: AppMenuManagementService,
        translate: TranslateService,
        router: Router,
        authenticationService: AuthenticationService,
        ngZone: NgZone,
        dialog: MatDialog) {
            super(translate, router, authenticationService, ngZone, dialog)
        }
    
    ngOnInit(): void {
        this.appMenuService.getAllMenus()
            .pipe(untilDestroyed(this))
            .subscribe(menus => this.menus = menus.sort((m1, m2) => {
                if (m1.builtin != m2.builtin) {
                    if (m1.builtin) return -1;
                    else if (m2.builtin) return 1;
                }
                return (m1.id - m2.id);
            }));

        this.appMenuService.getAllApps()
            .pipe(untilDestroyed(this))
            .subscribe(apps => {
                this.apps = apps;
                for(const app of apps) {
                    this.appMenuService.getAllAppMenus(app.id)
                        .pipe(untilDestroyed(this))
                        .subscribe(appsMenus => this.appsMenus[app.id] = appsMenus);
                }
        });
    }

    displayMode(displayModeId: number) {
        return Object.values(EMenuDisplayMode)[displayModeId];
    }
}