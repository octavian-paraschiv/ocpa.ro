import { Component, OnInit } from '@angular/core';
import { ActivationStart } from '@angular/router';
import { fas, faEarth } from '@fortawesome/free-solid-svg-icons';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { filter, map, switchMap } from 'rxjs/operators';
import { BaseComponent } from 'src/app/components/base/BaseComponent';
import { MessageBoxComponent } from 'src/app/components/shared/message-box/message-box.component';
import { Helper } from 'src/app/helpers/helper';
import { MessageBoxOptions } from 'src/app/models/models-local';
import { Menu } from 'src/app/models/models-swagger';
import { MenuService } from 'src/app/services/api/menu.service';

@UntilDestroy()
@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html'
})
export class NavMenuComponent extends BaseComponent implements OnInit {
    icons = fas;
    faEarth = faEarth;
    title = 'OcPa\'s Web Site';
    menus: Menu[] = [];
    authMenus: Menu[] = [];

    ngOnInit(): void {
        this.router.events
            .pipe(
              filter(e => e instanceof ActivationStart),
              map(e => e as ActivationStart)
            )
            .subscribe(activation => this.title = Helper.translateTitle(activation.snapshot, this.translate));

        this.authService.userLoginState$.pipe(
          untilDestroyed(this),
          switchMap(_ => this.menuService.init())
        ).subscribe(_ => {
          const menus = this.authService.isUserLoggedIn() ?
            this.menuService.menus.appMenus ?? [] :
            this.menuService.menus.publicMenus ?? [];

          for(const m of menus) {
            const key = `menu${m.url}`.replace(/\//g, '.');
            const translatedName = this.translate.instant(key);
            if (translatedName?.length > 0 && translatedName !== key) {
              m.name = translatedName;
            }
          }

          this.authMenus = menus.filter(m => MenuService.isAuthMenu(m));
          this.menus = menus.filter(m => this.authMenus.indexOf(m) < 0);
        });
    }

  logout() {
    MessageBoxComponent.show(this.dialogBS, {
      title: this.translate.instant('title.confirm'),
      message: this.translate.instant('auth.logout-desc'),

    } as MessageBoxOptions)
    .pipe(untilDestroyed(this))
    .subscribe(res => {
      if (res) {
        this.authService.logout(true);
      }
    });
  }

  isAuthMenu(m: Menu) {
    return MenuService.isAuthMenu(m);
  }
}
