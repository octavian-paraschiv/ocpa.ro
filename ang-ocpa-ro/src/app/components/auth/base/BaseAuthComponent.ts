import { Component, NgZone } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { first, map } from 'rxjs/operators';
import { BaseLifecycleComponent } from 'src/app/components/BaseLifecycleComponent';
import { MessageBoxComponent, MessageBoxOptions } from 'src/app/components/shared/message-box/message-box.component';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { Helper } from 'src/app/services/helper';

@UntilDestroy()
@Component({ template: '' })
export abstract class BaseAuthComponent extends BaseLifecycleComponent {
    static dialogRef: MatDialogRef<MessageBoxComponent> = undefined;
    dialogTimeout: any = undefined;

    constructor(
        private router: Router,
        protected authenticationService: AuthenticationService,
        private ngZone: NgZone,
        protected dialog: MatDialog
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

    protected onDestroy(): void {
        clearInterval(this.dialogTimeout);
    }

    protected onAfterViewInit() {
        clearInterval(this.dialogTimeout);
        this.dialogTimeout = setInterval(() => {
            console.debug('onAfterViewInit -> setInterval');

            if (this.authenticationService.isSessionExpired() && !BaseAuthComponent.dialogRef) {

                console.debug('onAfterViewInit -> setInterval -> session expired');

                BaseAuthComponent.dialogRef = this.dialog?.open(MessageBoxComponent, { 
                    data: {
                        isSessionTimeoutMessage: true,
                        noTimeout: 15000,
                        title: 'Confirm',
                        message: 'Your session has expired.<br>Press Yes if you want to stay logged in, and No otherwise.'
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
                                .pipe(
                                    first(), 
                                    untilDestroyed(this)
                                )
                                .subscribe({
                                    next: msg => {
                                        (msg?.length > 0) ? this.doLogout() : this.stayLoggedIn();
                                    },
                                    error: (err) => {
                                        this.doLogout();
                                    }
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

