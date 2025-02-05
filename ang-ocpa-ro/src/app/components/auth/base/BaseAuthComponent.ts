import { Component, OnDestroy, AfterViewInit, NgZone } from '@angular/core';
import { MatDialogRef, MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { TranslateService } from '@ngx-translate/core';
import { map, first } from 'rxjs/operators';
import { MessageBoxComponent } from 'src/app/components/shared/message-box/message-box.component';
import { Helper } from 'src/app/helpers/helper';
import { MessageBoxOptions } from 'src/app/models/models-local';
import { AuthenticationService } from 'src/app/services/api/authentication.services';

@UntilDestroy()
@Component({ template: '' })
export abstract class BaseAuthComponent implements OnDestroy, AfterViewInit {
    static dialogRef: MatDialogRef<MessageBoxComponent> = undefined;
    dialogTimeout: any = undefined;

    constructor(
        protected translate: TranslateService,
        private router: Router,
        protected authenticationService: AuthenticationService,
        private ngZone: NgZone,
        protected dialog: MatDialog
    ) { 
        if (Helper.isMobile())
            this.router.navigate(['/meteo']); // Forbid Admin mode when using a mobile device

        else if (!this.authenticationService.isUserLoggedIn())
            this.router.navigate(['/login']);
    }

    logout() {
        this.authenticationService.logout(true);
    }

    ngOnDestroy(): void {
        clearInterval(this.dialogTimeout);
    }

    ngAfterViewInit() {
        clearInterval(this.dialogTimeout);
        this.dialogTimeout = setInterval(() => {
            console.debug('onAfterViewInit -> setInterval');

            if (this.authenticationService.isSessionExpired() && !BaseAuthComponent.dialogRef) {

                console.debug('onAfterViewInit -> setInterval -> session expired');

                BaseAuthComponent.dialogRef = this.dialog?.open(MessageBoxComponent, { 
                    data: {
                        isSessionTimeoutMessage: true,
                        noTimeout: 15000,
                        title: this.translate.instant('title.confirm'),
                        message: this.translate.instant('auth.session-expired'),
                        
                    } as MessageBoxOptions,
                    panelClass: 'session-expired-message'
                });

                BaseAuthComponent.dialogRef.afterClosed()
                    .pipe(
                        untilDestroyed(this), 
                        map(result => result as boolean)
                    )
                    .subscribe(res => {
                        BaseAuthComponent.dialogRef = undefined;
                        if (res) {
                            this.authenticationService.refreshAuthentication()
                                .pipe(first(), untilDestroyed(this))
                                .subscribe({
                                    next: msg => (msg?.length > 0) ? this.doLogout() : this.stayLoggedIn(),
                                    error: () => this.doLogout()
                                });
                        } else {
                            this.doLogout();
                        }
                    });
            }

        }, 3000);
    }

    doLogout() {
        this.ngZone.run(() => this.logout());
    }

    protected stayLoggedIn() {
    }
}

