import {Component, Inject, OnInit} from '@angular/core';
import {MatDialog, MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AuthenticationService } from 'src/app/services/authentication.services';

export interface MessageBoxOptions {
    message: string;
    title: string;
    icon: string;
    yesButtonText?: string;
    noButtonText?: string;
    noTimeout?: number;
    yesTimeout?: number;
    isSessionTimeoutMessage?: boolean;
}

@UntilDestroy()
@Component({
    selector: 'app-message-box',
    templateUrl: './message-box.component.html'
})
export class MessageBoxComponent implements OnInit {

    constructor(
        private authService: AuthenticationService,
        public dialogRef: MatDialogRef<MessageBoxComponent>,
        @Inject(MAT_DIALOG_DATA) public options: MessageBoxOptions
    ) {
    }

    ngOnInit(): void {
        if (!this.options?.isSessionTimeoutMessage) {
            this.authService.authUserChanged$
            .pipe(untilDestroyed(this))
            .subscribe(() => {
                if(!this.authService.isUserLoggedIn())
                    this.onNo();
            });
        }

        if (this.options?.noTimeout > 0)
            // Set a timeout to close the dialog with "no" action
            setTimeout(() => this.onNo(), this.options.noTimeout);

        if (this.options?.yesTimeout > 0)
            // Set a timeout to close the dialog with "yes" action
            setTimeout(() => this.onYes(), this.options.yesTimeout);
    }

    onNo(): void {
        this.dialogRef.close(false);
    }

    onYes(): void {
        this.dialogRef.close(true);
    }

    static show(dialog: MatDialog, options: MessageBoxOptions): Observable<boolean> {
        const dialogRef = dialog?.open(MessageBoxComponent, { data: options });
        return dialogRef.afterClosed().pipe(map(result => result as boolean));
    }
}
