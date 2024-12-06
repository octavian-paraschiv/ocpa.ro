import { Component } from '@angular/core';
import { Router, ActivationStart } from '@angular/router';
import { filter, switchMap, map } from 'rxjs/operators';
import { fas, faEarth } from '@fortawesome/free-solid-svg-icons';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { MenuService } from 'src/app/services/menu.service';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Menu } from 'src/app/models/models-swagger';
import { TranslateService } from '@ngx-translate/core';

@UntilDestroy()
@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html'
})
export class NavMenuComponent {
    icons = fas;
    faEarth = faEarth;
    title = 'OcPa\'s Web Site';
    menus: Menu[] = [];

    constructor(
      private readonly translate: TranslateService,
      private readonly router: Router,
      private readonly authService: AuthenticationService,
      private readonly menuService: MenuService
    ) {
        this.router.events
            .pipe(
              filter(e => e instanceof ActivationStart),
              map(e => e as ActivationStart)
            )
            .subscribe(activation => {
                try { 
                  const url = activation?.snapshot?.url;
                  const path = activation?.snapshot?.routeConfig?.path;
                  let rawTitle = 'meteo';
                  if (url?.length > 0 && path !== '**')
                    rawTitle = url.map(s => s.path).join('.');

                  this.title = this.translate.instant(`title.${rawTitle}`);
                }
                catch { }
            });

        this.authService.authUserChanged$.pipe(
          untilDestroyed(this),
          switchMap(_ => this.menuService.init())
        ).subscribe(_ => {
          const menus = this.authService.isUserLoggedIn() ?
            this.menuService.menus.appMenus ?? [] :
            this.menuService.menus.publicMenus ?? [];

          for(const m of menus)
            m.name = this.translate.instant(`menu${m.url}`.replace(/\//g, '.'));

          this.menus = menus;
        });
    }

  logout() {
    this.authService.logout(true);
  }

}
