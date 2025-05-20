import { Component, OnInit } from '@angular/core';
import { ActivationStart } from '@angular/router';
import { fas, faEarth } from '@fortawesome/free-solid-svg-icons';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { filter, map, switchMap } from 'rxjs/operators';
import { BaseComponent } from 'src/app/components/base/BaseComponent';
import { Helper } from 'src/app/helpers/helper';
import { Menu } from 'src/app/models/models-swagger';

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
    singleMenuApp = false;

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

          this.menus = menus;
          this.singleMenuApp = this.menuService.singleMenuApp$.getValue();
        });
    }

  logout() {
    this.authService.logout(true);
  }

}
