import { Component, OnInit } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Subject } from 'rxjs';
import { AuthenticationService } from 'src/app/services/api/authentication.services';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';

export interface MessageBoxOptions {
    message: string;
    title: string;
    icon: string;
    yesButtonText?: string;
    noButtonText?: string;
    noTimeout?: number;
    yesTimeout?: number;
    isSessionTimeoutMessage?: boolean;
    panelClass?: string;
}

@UntilDestroy()
@Component({
  selector: 'app-message-box',
  templateUrl: './message-box.component.html'
})
export class MessageBoxComponent implements OnInit {
  options: MessageBoxOptions;
  $result = new Subject<boolean>();

  constructor(
    private authService: AuthenticationService,
    public bsModalRef: BsModalRef
  ) {}

  ngOnInit(): void {
    if (!this.options?.isSessionTimeoutMessage) {
      this.authService.userLoginState$
        .pipe(untilDestroyed(this))
        .subscribe(() => {
          if (!this.authService.isUserLoggedIn()) {
            this.onNo();
          }
        });
    }

    if (this.options?.noTimeout > 0) {
      setTimeout(() => this.onNo(), this.options.noTimeout);
    }

    if (this.options?.yesTimeout > 0) {
      setTimeout(() => this.onYes(), this.options.yesTimeout);
    }
  }

  onNo(): void {
    this.$result.next(false);
    this.bsModalRef.hide();
  }

  onYes(): void {
    this.$result.next(true);
    this.bsModalRef.hide();
  }

  static show(modalService: BsModalService, options: MessageBoxOptions): Subject<boolean> {
    const initialState = { options };
    const bsModalRef = modalService.show(MessageBoxComponent, {
      initialState,
      class: options.panelClass ?? 'bs-modal'
    });

    return (bsModalRef.content as MessageBoxComponent).$result;
  }
}
