import { Component, NgZone, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { interval } from 'rxjs';
import { AuthenticationService } from 'src/app/services/authentication.services';

@UntilDestroy()
@Component({
    selector: 'app-admin',
    templateUrl: './admin.component.html',
    styleUrls: ['../../../assets/styles/site.css']
})
export class AdminComponent implements OnInit, OnDestroy {

    constructor(
        private router: Router,
        private authenticationService: AuthenticationService,
        private ngZone: NgZone
    ) { 
        // redirect to login if not already logged in
        if (!this.authenticationService.currentUserValue || 
            !this.authenticationService.currentUserValue.token) { 
            this.router.navigate(['/login']);
        }
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
    }

    ngOnDestroy(): void {
        this.authenticationService.logout();
    }

    logout() {
        this.authenticationService.logout();
        this.router.navigate(['/login']);
    }

}
