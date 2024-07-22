import { NgZone, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { interval } from 'rxjs';
import { AuthenticationService } from 'src/app/services/authentication.services';

@UntilDestroy()
export abstract class BaseAuthComponent implements OnInit, OnDestroy {

    constructor(
        private router: Router,
        protected authenticationService: AuthenticationService,
        private ngZone: NgZone
    ) { 
        const ua = navigator.userAgent;
        if(/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|Mobile|mobile|CriOS/i.test(ua))
            this.router.navigate(['/']); // Forbid Admin mode when using a mobile device
        else if (!this.authenticationService.validAdminUser)
            this.router.navigate(['/login']);
    }

    ngOnInit() {
        this.ngZone.runOutsideAngular(() => {
            interval(500)
              .pipe(untilDestroyed(this))
              .subscribe(() => {
                if (this.authenticationService.isSessionExpired()) {
                  this.ngZone.run(() => this.logout());
                }
              });
          });

        this.onInit();
    }

    ngOnDestroy(): void {
        this.onDestroy();
    }

    logout() {
        this.authenticationService.logout(true);
    }

    protected onInit() {}
    protected onDestroy() {}
}
