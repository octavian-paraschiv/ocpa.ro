import { Component, OnInit, NgZone, inject, HostListener, ViewChild, AfterViewInit } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { BehaviorSubject } from 'rxjs';
import { first, map } from 'rxjs/operators';
import { BaseComponent } from 'src/app/components/base/BaseComponent';
import { MessageBoxComponent } from 'src/app/components/shared/message-box/message-box.component';
import { OverlayComponent } from 'src/app/components/shared/overlay/overlay.component';
import { Helper } from 'src/app/helpers/helper';
import { MessageBoxOptions } from 'src/app/models/models-local';
import { UtilityService } from 'src/app/services/api/utility.service';
import { OverlayService } from 'src/app/services/overlay.service';

@UntilDestroy()
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent extends BaseComponent implements OnInit, AfterViewInit {
  @ViewChild(OverlayComponent) overlayComponent: OverlayComponent;
  
  private dialogTimeout: any = undefined;
  private dialogRefBS: BsModalRef<MessageBoxComponent> = undefined;

  private readonly ngZone = inject(NgZone);
  private readonly utility = inject(UtilityService);
  private readonly overlayService = inject(OverlayService);

  private refreshTokenPending$ = new BehaviorSubject<boolean>(false);

  version = 'n/a';

  ngAfterViewInit(): void {
    this.overlayService.setOverlayComponent(this.overlayComponent);
  }

  ngOnInit() {
    this.utility.getBackendVersion()
      .pipe(untilDestroyed(this))
      .subscribe(res => this.version = (res ?? '').replace(/"/g, ''));

    this.onWindowResized(undefined);
    this.startRefreshAuthTimer();

    this.menuService.singleMenuApp$
      .pipe(untilDestroyed(this))
      .subscribe(res => document
        .getElementById('app-nav-menu')
        .setAttribute('single-menu-app', !!res ? 'true' : 'false'));

  }

  @HostListener('window:resize', ['$event'])
  onWindowResized(_event: any) {
    const root = document.documentElement;
    root.setAttribute('mobile', Helper.isMobile() ? 'true' : 'false');
    root.setAttribute('display-mode', Helper.displayMode());
  }

  private startRefreshAuthTimer() {
    clearInterval(this.dialogTimeout);
    this.dialogRefBS = undefined;
    
    this.dialogTimeout = setInterval(() => {
        console.debug('refreshAuthTimer -> setInterval');
        
        if (this.authService.isSessionExpirationPending() && 
            !this.dialogRefBS && 
            !this.refreshTokenPending$.getValue()) {
            console.debug('onAfterViewInit -> setInterval -> session expired');

            this.dialogRefBS = this.dialog?.show(MessageBoxComponent, { 
              initialState: {
                options: {
                  isSessionTimeoutMessage: true,
                  noTimeout: 15000,
                  title: this.translate.instant('title.confirm'),
                  message: this.translate.instant('auth.session-expired'),
                } as MessageBoxOptions
              },
              class: 'session-expired-message'
            });

            this.dialogRefBS.content.$result
              .pipe(untilDestroyed(this), map(result => result as boolean))
              .subscribe(res => {
                  this.dialogRefBS = undefined;
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

