import { formatDate } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { AbstractControl, UntypedFormBuilder, UntypedFormGroup, ValidatorFn, Validators } from '@angular/forms';
import { MatDialog, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { UserType, User, RegisteredDevice } from 'src/app/models/models-swagger';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { UserTypeService } from 'src/app/services/user-type.service';
import { environment } from 'src/environments/environment';

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
        this.authService.authUserChanged$
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

    get title(): string {
        return `Information for device <b>${this.device?.deviceId}</b>`;
    }

    static showDialog(dialog: MatDialog, device: RegisteredDevice = undefined): Observable<any> {
        const dialogRef = dialog?.open(DevicesDialogComponent, { data: device });
        return dialogRef.afterClosed();
    }
}
