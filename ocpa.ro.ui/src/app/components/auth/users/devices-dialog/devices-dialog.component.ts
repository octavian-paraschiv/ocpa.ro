import { formatDate } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Observable, Subject } from 'rxjs';
import { RegisteredDevice } from 'src/app/models/swagger/access-management';
import { AuthenticationService } from 'src/app/services/api/authentication.services';

@UntilDestroy()
@Component({
    selector: 'devices-dialog.component',
    templateUrl: './devices-dialog.component.html'
})
export class DevicesDialogComponent implements OnInit {
    @Input() device: RegisteredDevice;
    deviceInfo: any = undefined;
    result$: Subject<any> = new Subject<any>();

    constructor(
        private authService: AuthenticationService,
        public bsModalRef: BsModalRef<DevicesDialogComponent>
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
        this.bsModalRef.hide();
        this.result$.complete();
    }

    static showDialog(modalService: BsModalService, device: RegisteredDevice = undefined): Observable<any> {
        const bsModalRef: BsModalRef<DevicesDialogComponent> = modalService.show(DevicesDialogComponent, {
            initialState: { device },
            class: 'bs-modal'      
        });

        return bsModalRef.content.result$;
    }
}
