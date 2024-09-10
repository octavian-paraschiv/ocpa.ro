import { Component, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { untilDestroyed } from '@ngneat/until-destroy';
import { interval } from 'rxjs';
import { BaseLifecycleComponent } from 'src/app/components/BaseLifecycleComponent';
import { AuthenticationService } from 'src/app/services/authentication.services';

@Component({ template: '' })
export abstract class BaseAuthComponent extends BaseLifecycleComponent {

    constructor(
        private router: Router,
        protected authenticationService: AuthenticationService,
        private ngZone: NgZone
    ) { 
        super();

        const ua = navigator.userAgent;
        if(/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|Mobile|mobile|CriOS/i.test(ua))
            this.router.navigate(['/']); // Forbid Admin mode when using a mobile device
        else if (!this.authenticationService.validAdminUser)
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

