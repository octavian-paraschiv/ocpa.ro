import {Component, Inject} from '@angular/core';
import {MatDialog, MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface MessageBoxOptions {
    message: string;
    title: string;
    icon: string;
    yesButtonText?: string;
    noButtonText?: string;
}

@Component({
    selector: 'app-message-box',
    templateUrl: './message-box.component.html'
})
export class MessageBoxComponent {

    constructor(
        public dialogRef: MatDialogRef<MessageBoxComponent>,
        @Inject(MAT_DIALOG_DATA) public options: MessageBoxOptions
    ) {
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
