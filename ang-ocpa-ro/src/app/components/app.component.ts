import { Component, OnInit, NgZone, inject, HostListener } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { BehaviorSubject } from 'rxjs';
import { first, map } from 'rxjs/operators';
import { BaseComponent } from 'src/app/components/base/BaseComponent';
import { MessageBoxComponent } from 'src/app/components/shared/message-box/message-box.component';
import { Helper } from 'src/app/helpers/helper';
import { MessageBoxOptions } from 'src/app/models/models-local';
import { UtilityService } from 'src/app/services/api/utility.service';

@UntilDestroy()
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent extends BaseComponent implements OnInit {
  private dialogTimeout: any = undefined;
  private dialogRef: MatDialogRef<MessageBoxComponent> = undefined;

  private readonly ngZone = inject(NgZone);
  private readonly utility = inject(UtilityService);

  private refreshTokenPending$ = new BehaviorSubject<boolean>(false);

  version = 'n/a';

  ngOnInit() {
    this.utility.getBackendVersion()
      .pipe(untilDestroyed(this))
      .subscribe(res => this.version = (res ?? '').replace(/"/g, ''));

    this.menuService.singleMenuApp$
      .pipe(untilDestroyed(this))
      .subscribe(res => this.setSingleMenuApp(res));

    this.onWindowResized(undefined);
    this.setSingleMenuApp(this.menuService.singleMenuApp$.getValue());
    this.startRefreshAuthTimer();
  }

  @HostListener('window:resize', ['$event'])
  onWindowResized(_event: any) {
    const root = document.documentElement;
    root.setAttribute('mobile', Helper.isMobile() ? 'true' : 'false');
    root.setAttribute('display-mode', Helper.displayMode());
  }

  private setSingleMenuApp(singleMenuApp: boolean) {
    const root = document.documentElement;
    root.setAttribute('single-menu-app', singleMenuApp ? 'true' : 'false');
  }

  private startRefreshAuthTimer() {
    clearInterval(this.dialogTimeout);
    this.dialogRef = undefined;
    
    this.dialogTimeout = setInterval(() => {
        console.debug('refreshAuthTimer -> setInterval');
        
        if (this.authService.isSessionExpirationPending() && 
            !this.dialogRef && 
            !this.refreshTokenPending$.getValue()) {
            console.debug('onAfterViewInit -> setInterval -> session expired');
            this.dialogRef = this.dialog?.open(MessageBoxComponent, { 
                data: {
                    isSessionTimeoutMessage: true,
                    noTimeout: 15000,
                    title: this.translate.instant('title.confirm'),
                    message: this.translate.instant('auth.session-expired'),
                    
                } as MessageBoxOptions,
                panelClass: 'session-expired-message'
            });

            this.dialogRef.afterClosed()
                .pipe(untilDestroyed(this), map(result => result as boolean))
                .subscribe(res => {
                    this.dialogRef = undefined;
                    if (res) {
                        this.refreshTokenPending$.next(true);
                        this.authService.refreshAuthentication()
                            .pipe(first(), untilDestroyed(this))
                            .subscribe({
                                next: msg => (msg?.length > 0) ? this.doLogout() : {},
                                error: () => this.doLogout(),
                                complete: () => this.refreshTokenPending$.next(false)
                            });
                    } else {
                        this.doLogout();
                    }
                });
        }

    }, 3000);
  }

  doLogout() {
    this.ngZone.run(() => this.authService.logout(true));
  }
}

