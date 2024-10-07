import { Component, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { untilDestroyed } from '@ngneat/until-destroy';
import { interval } from 'rxjs';
import { BaseLifecycleComponent } from 'src/app/components/BaseLifecycleComponent';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { Helper } from 'src/app/services/helper';

@Component({ template: '' })
export abstract class BaseAuthComponent extends BaseLifecycleComponent {

    constructor(
        private router: Router,
        protected authenticationService: AuthenticationService,
        private ngZone: NgZone
    ) { 
        super();

        if (Helper.isMobile())
            this.router.navigate(['/meteo']); // Forbid Admin mode when using a mobile device

        else if (!this.authenticationService.isUserLoggedIn())
            this.router.navigate(['/login']);
    }

    logout() {
        this.authenticationService.logout(true);
    }

    protected onInit() {
        this.ngZone.runOutsideAngular(() => {
            interval(500)
              .pipe(untilDestroyed(this))
              .subscribe(() => {
                if (this.authenticationService.isSessionExpired()) {
                  this.ngZone.run(() => this.logout());
                }
              });
          });
    }
}

