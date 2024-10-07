import { Component } from '@angular/core';
import { Router, ActivationStart } from '@angular/router';
import { filter, switchMap } from 'rxjs/operators';
import { fas, faEarth } from '@fortawesome/free-solid-svg-icons';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { MenuService } from 'src/app/services/menu.service';
import { Menu } from 'src/app/models/models-swagger';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';

@UntilDestroy()
@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html'
})
export class NavMenuComponent {
    icons = fas;
    faEarth = faEarth;

    title = 'OcPa\'s Web Site';
    path: string = 'ocpa';
    menus: Menu[] = [];

    constructor(private readonly router: Router,
      private readonly authService: AuthenticationService,
      private readonly menuService: MenuService
    ) {
        this.router.events
            .pipe(filter(e => e instanceof ActivationStart))
            .subscribe(e => {
                try { 
                  this.title = (e as ActivationStart).snapshot.data['title']; 
                  const path = (e as ActivationStart).snapshot.routeConfig.path;
                  if (path && path.length > 0) {
                    this.path = path;
                  } else {
                    this.path = 'ocpa';
                  }
                }
                catch { }
            });

        this.authService.authUserChanged$.pipe(
          untilDestroyed(this),
          switchMap(_ => this.menuService.init())
        ).subscribe(_ => {
          this.menus = this.authService.isUserLoggedIn() ?
            this.menuService.menus.appMenus ?? [] :
            this.menuService.menus.publicMenus ?? [];
        });
    }

  logout() {
    this.authService.logout(true);
  }

}
