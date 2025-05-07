import { Component, OnInit, inject } from '@angular/core';
import { fas, faSquarePlus, faSquarePen, faSquareMinus, faCheck } from '@fortawesome/free-solid-svg-icons';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { of, forkJoin, Observable } from 'rxjs';
import { first, tap, switchMap } from 'rxjs/operators';
import { AppDialogComponent } from 'src/app/components/auth/apps-menus/app-dialog/app-dialog.component';
import { MenuDialogComponent } from 'src/app/components/auth/apps-menus/menu-dialog/menu-dialog.component';
import { BaseAuthComponent } from 'src/app/components/base/BaseComponent';
import { MessageBoxComponent } from 'src/app/components/shared/message-box/message-box.component';
import { AppMenuData, AppData, MessageBoxOptions } from 'src/app/models/models-local';
import { Menu, Application, ApplicationMenu, EMenuDisplayMode } from 'src/app/models/models-swagger';
import { AppMenuManagementService } from 'src/app/services/api/app-menu-management.service';
import { MessagePopupService } from 'src/app/services/message-popup.service';

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

    private readonly appMenuService = inject(AppMenuManagementService);

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
        
        this.overlay.show();

        obs.pipe(first(), untilDestroyed(this)).subscribe({
            next: () => this.popup.showSuccess(key),
            error: () => this.popup.showError(keyErr)
        });
    }

    ngOnInit(): void {
        this.onInit();
    }

    onInit() {
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

        this.overlay.show();

        forkJoin([o1, o2, o3])
            .pipe(untilDestroyed(this))
            .subscribe({
                next: () => {
                    this.overlay.hide();
                    this.appMenuData = this.menus.map(menu => ({
                        menuId: menu.id,
                        menuName: menu.name,
                        appData: this.apps.map(app => ({
                            appId: app.id,
                            appName: app.name,
                            active: this.appsMenus.find(am => am.applicationId === app.id && am.menuId === menu.id)?.id > 0
                        }) as AppData)
                    }) as AppMenuData);
                },
                error: () => this.overlay.hide()
        });
    }

    displayMode(displayModeId: number) {
        return Object.values(EMenuDisplayMode)[displayModeId];
    }

    deleteMenu(menu: Menu) {
        MessageBoxComponent.show(this.dialog, {
            title: this.translate.instant('title.confirm'),
            message: this.translate.instant('apps-menus.delete-menu', { name: menu.name })
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                this.overlay.show();
                this.appMenuService.deleteMenu(menu.id)
                .pipe(untilDestroyed(this))
                .subscribe({
                    next: () => {
                        this.onInit();
                        this.popup.showSuccess('apps-menus.success-delete-menu', { name: menu.name });
                    },
                    error: err => {
                        this.popup.showError(err.toString(), { name: menu.name });
                    }
                });
            }
        });
    }

    deleteApp(app: Application) {
        MessageBoxComponent.show(this.dialog, {
            title: this.translate.instant('title.confirm'),
            message: this.translate.instant('apps-menus.delete-app', { name: app.name })
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                this.overlay.show();
                this.appMenuService.deleteApp(app.id)
                .pipe(untilDestroyed(this))
                .subscribe({
                    next: () => {
                        this.onInit();
                        this.popup.showSuccess('apps-menus.success-delete-app', { name: app.name });
                    },
                    error: err => {
                        this.popup.showError(err.toString(), { name: app.name });
                    }
                });
            }
        });
    }

    saveMenu(menu: Menu = undefined) {
        MenuDialogComponent.showDialog(this.dialog, menu)
        .pipe(
            untilDestroyed(this),
            switchMap(menu => menu?.id === -1 ? of(menu) : this._saveMenu(menu))            
        ).subscribe({
            next: menu => {
                if (menu) {
                    if (menu.id > 0) {
                        this.onInit();
                        this.popup.showSuccess('apps-menus.success-save-menu', { name: menu.name });
                    }
                } else {
                    this.popup.showError('apps-menus.error-save-menu');
                }
            },
            error: err => {
                this.popup.showError(err.toString(), { name: menu.name });
            }
        });
    }

    _saveMenu(menu: Menu): Observable<Menu> {
        this.overlay.show();
        return this.appMenuService.saveMenu(menu);
    }

    saveApp(app: Application = undefined) {
        AppDialogComponent.showDialog(this.dialog, app)
        .pipe(
            untilDestroyed(this),
            switchMap(app => app?.id === -1 ? of(app) : this._saveApp(app))
        ).subscribe({
            next: app => {
                if (app) {
                    if (app.id > 0) {
                        this.onInit();
                        this.popup.showSuccess('apps-menus.success-save-app', { name: app.name });
                    }
                } else {
                    this.popup.showError('apps-menus.error-save-app');
                }
            },
            error: err => {
                this.popup.showError(err.toString(), { name: app.name });
            }
        });
    }

    _saveApp(app: Application): Observable<Application> {
        this.overlay.show();
        return this.appMenuService.saveApp(app);
    }
}