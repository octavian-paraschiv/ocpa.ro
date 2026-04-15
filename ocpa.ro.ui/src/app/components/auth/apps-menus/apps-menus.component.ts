import { Component, OnInit, inject } from '@angular/core';
import { fas, faSquarePlus, faSquareMinus, faCheck, faSquarePen } from '@fortawesome/free-solid-svg-icons';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { of, forkJoin, Observable } from 'rxjs';
import { first, tap, switchMap } from 'rxjs/operators';

import { AppDialogComponent } from './app-dialog/app-dialog.component';
import { MenuDialogComponent } from './menu-dialog/menu-dialog.component';
import { BaseAuthComponent } from '../../base/BaseComponent';
import { MessageBoxComponent, MessageBoxOptions } from '../../shared/message-box/message-box.component';

import { AppMenuData, AppData } from 'src/app/models/local/access-management';
import { Menu, Application, ApplicationMenu, EMenuDisplayMode } from 'src/app/models/swagger/access-management';
import { AppMenuManagementService } from 'src/app/services/api/app-menu-management.service';

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
  size = 'grow-9';

  private readonly appMenuService = inject(AppMenuManagementService);

  menus: Menu[] = [];
  apps: Application[] = [];
  appsMenus: ApplicationMenu[] = [];
  appMenuData: AppMenuData[] = [];

  menusColumns = ['menu-add', 'menu-edit', 'menu-delete', 'menu-name', 'menu-url', 'menu-display-mode', 'menu-icon', 'menu-builtin' ];
  appsColumns = ['app-add', 'app-edit', 'app-delete', 'app-name', 'app-code', 'app-login-required', 'app-admin-mode', 'app-builtin' ];

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
    const menuId = appMenu.menuId;
    const appId = appMenu.appData.find(ad => ad.appName === appName)?.appId;
    let obs: Observable<any> = of();
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

    const o1 = this.appMenuService.getAllMenus().pipe(
      first(),
      tap(menus => this.menus = menus.sort((m1, m2) => m1.builtin !== m2.builtin ? (m1.builtin ? -1 : 1) : m1.id - m2.id)),
      untilDestroyed(this)
    );

    const o2 = this.appMenuService.getAllApps().pipe(
      first(),
      tap(apps => this.apps = apps.sort((a1, a2) => a1.builtin !== a2.builtin ? (a1.builtin ? -1 : 1) : a1.id - a2.id)),
      untilDestroyed(this)
    );

    const o3 = this.appMenuService.getAllAppMenus().pipe(
      first(),
      tap(appsMenus => this.appsMenus = appsMenus),
      untilDestroyed(this)
    );

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
      )
      .subscribe({
        next: menu => {
          if (menu) {
              if (menu.id > 0) {
                  this.onInit();
                  this.popup.showSuccess('apps-menus.success-save-menu', { name: menu.name });

              } else {
                // Do nothing if menu.id < 0 - it means that we cancelled editing the menu
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

  private _saveMenu(menu: Menu): Observable<Menu> {
    this.overlay.show();
    return this.appMenuService.saveMenu(menu);
  }

  saveApp(app: Application = undefined) {
    AppDialogComponent.showDialog(this.dialog, app)
      .pipe(
        untilDestroyed(this),
        switchMap(app => app?.id === -1 ? of(app) : this._saveApp(app))
      )
      .subscribe({
        next: app => {
          if (app) {
            if (app.id > 0) {
              this.onInit();
              this.popup.showSuccess('apps-menus.success-save-app', { name: app.name });

             } else {
                // Do nothing if app.id < 0 - it means that we cancelled editing the app
              }
          }
          else {
            this.popup.showError('apps-menus.error-save-app');
          }
        },
        error: err => {
          this.popup.showError(err.toString(), { name: app.name });
        }
      });
  }

  private _saveApp(app: Application): Observable<Application> {
    this.overlay.show();
    return this.appMenuService.saveApp(app);
  }
}