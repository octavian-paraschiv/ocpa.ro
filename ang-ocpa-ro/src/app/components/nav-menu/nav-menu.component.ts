import { Component } from '@angular/core';
import { Router, ActivationStart } from '@angular/router';
import { fas, faEarth } from '@fortawesome/free-solid-svg-icons';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { TranslateService } from '@ngx-translate/core';
import { filter, map, switchMap } from 'rxjs/operators';
import { Menu } from 'src/app/models/models-swagger';
import { translateTitle } from 'src/app/modules/module.routes';
import { AuthenticationService } from 'src/app/services/api/authentication.services';
import { MenuService } from 'src/app/services/api/menu.service';

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
    singleMenuApp = false;

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
            .subscribe(activation => this.title = translateTitle(activation.snapshot, translate));

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
          this.singleMenuApp = this.menuService.singleMenuApp$.getValue();
        });
    }

  logout() {
    this.authService.logout(true);
  }

}
