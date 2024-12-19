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
import { first, tap } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material/snack-bar';
import { forkJoin, of } from 'rxjs';

export interface AppMenuData {
    menuId: number;
    menuName: string;
    appData: AppData[];
}

export interface AppData {
    appName: string;
    appId: number;
    active: boolean;
  }
  
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

    appsMenus: ApplicationMenu[] = [];
    appMenuData: AppMenuData[] = [];

    get appsMenusColumns() {
        return ['app-menu-name', ...this.appNames.map(an => `app-menu-${an}`)];
    }
    get appNames() {
        const x = this.appMenuData.flatMap(appMenu => appMenu.appData).map(appData => appData.appName);
        return Array.from(new Set(x));
    }

    state(appMenu: AppMenuData, appName: string): boolean {
        return appMenu.appData.find(ad => ad.appName === appName)?.active;
    }

    onAppMenuChange(appMenu: AppMenuData, appName: string, checked: boolean) {
        const menuId = appMenu.     menuId;
        const appId = appMenu.appData.find(ad => ad.appName === appName)?.appId;
        
        let obs = of();
        let key = '';
        let keyErr = '';
            
        if (checked) {
            obs = this.appMenuService.saveAppMenu(appId, menuId);
            key = 'apps-menus.menu-assoc-created';
            keyErr = 'apps-menus.menu-assoc-create-err';
        } else {
            obs = this.appMenuService.deleteAppMenu(appId, menuId);
            key = 'apps-menus.menu-assoc-remove';
            keyErr = 'apps-menus.menu-assoc-remove-err';
        }
        
        obs.pipe(first(), untilDestroyed(this)).subscribe({
            next: () => this.snackBar.open(this.translate.instant(key), undefined, { duration: 5000 }),
            error: () => this.snackBar.open(this.translate.instant(keyErr), undefined, { duration: 5000 })
        });
    }

    constructor(private readonly appMenuService: AppMenuManagementService,
        translate: TranslateService,
        router: Router,
        authenticationService: AuthenticationService,
        ngZone: NgZone,
        dialog: MatDialog,
        private readonly snackBar: MatSnackBar) {
            super(translate, router, authenticationService, ngZone, dialog)
        }
    
    ngOnInit(): void {
        this.appMenuData = [];

        const o1 = this.appMenuService.getAllMenus()
            .pipe(
                first(), 
                tap(menus => this.menus = menus.sort((m1, m2) => {
                    if (m1.builtin != m2.builtin) {
                        if (m1.builtin) return -1;
                        else if (m2.builtin) return 1;
                    }
                    return (m1.id - m2.id);
                })), 
                untilDestroyed(this));

        const o2 = this.appMenuService.getAllApps()
            .pipe(
                first(), 
                tap(apps => this.apps = apps.sort((m1, m2) => {
                    if (m1.builtin != m2.builtin) {
                        if (m1.builtin) return -1;
                        else if (m2.builtin) return 1;
                    }
                    return (m1.id - m2.id);
                })), 
                untilDestroyed(this));

        const o3 = this.appMenuService.getAllAppMenus()
            .pipe(
                first(), 
                tap(appsMenus => this.appsMenus = appsMenus), 
                untilDestroyed(this));

        forkJoin([o1, o2, o3])
            .pipe(untilDestroyed(this))
            .subscribe(() => {
                this.appMenuData = this.menus.map(menu => ({
                    menuId: menu.id,
                    menuName: menu.name,
                    appData: this.apps.map(app => ({
                        appId: app.id,
                        appName: app.name,
                        active: this.appsMenus.find(am => am.applicationId === app.id && am.menuId === menu.id)?.id > 0
                    }) as AppData)
                }) as AppMenuData);
            });
    }

    displayMode(displayModeId: number) {
        return Object.values(EMenuDisplayMode)[displayModeId];
    }
}