import { formatDate } from '@angular/common';
import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { RegisteredDevice } from 'src/app/models/models-swagger';
import { AuthenticationService } from 'src/app/services/api/authentication.services';

@UntilDestroy()
@Component({
    selector: 'devices-dialog.component',
    templateUrl: './devices-dialog.component.html'
})
export class DevicesDialogComponent implements OnInit {
    deviceInfo: any = undefined;
    constructor(
        private authService: AuthenticationService,
        public dialogRef: MatDialogRef<DevicesDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public device: RegisteredDevice
    ) {
    }

    ngOnInit() {
        this.authService.userLoginState$
        .pipe(untilDestroyed(this))
        .subscribe(() => {
            if(!this.authService.isUserLoggedIn())
                this.onClose();
        });

        const obj1 = {
            deviceId: this.device.deviceId,
            lastLoginId: this.device.lastLoginId,
            lastLoginTimestamp: formatDate(this.device.lastLoginTimestamp, 'yyyy-MM-dd HH:mm:ss', 'en-US'),
            lastLoginIpAddress: this.device.lastLoginIpAddress
        };

        let obj2 = undefined;
        try {
            obj2 = JSON.parse(this.device.lastLoginGeoLocation);
        } catch {
            obj2 = undefined;
        }

        this.deviceInfo = { ...obj1, ...obj2 };
    }

    onClose() {
        this.dialogRef.close();
    }

    static showDialog(dialog: MatDialog, device: RegisteredDevice = undefined): Observable<any> {
        const dialogRef = dialog?.open(DevicesDialogComponent, { data: device });
        return dialogRef.afterClosed();
    }
}
