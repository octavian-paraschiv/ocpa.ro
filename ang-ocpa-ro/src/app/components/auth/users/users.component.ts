import { Component, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { BaseAuthComponent } from 'src/app/components/auth/base/BaseAuthComponent';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { UserService } from 'src/app/services/user.service';
import { faEye, faSquarePlus, faSquarePen, faSquareMinus } from '@fortawesome/free-solid-svg-icons';
import { MatDialog } from '@angular/material/dialog';
import { UserDialogComponent } from 'src/app/components/auth/users/user-dialog/user-dialog.component';
import { switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { UserTypeService } from 'src/app/services/user-type.service';
import { MessageBoxComponent, MessageBoxOptions } from 'src/app/components/shared/message-box/message-box.component';
import { RegisteredDevice, User } from 'src/app/models/models-swagger';
import { RegisteredDeviceService } from 'src/app/services/registered-device.service';
import { DevicesDialogComponent } from 'src/app/components/auth/users/devices-dialog/devices-dialog.component';
import { TranslateService } from '@ngx-translate/core';
import { MessagePopupService } from 'src/app/services/message-popup.service';

@UntilDestroy()
@Component({
    selector: 'app-users',
    templateUrl: './users.component.html'
})
export class UsersComponent extends BaseAuthComponent {
    faEye = faEye;
    faAdd = faSquarePlus;
    faEdit = faSquarePen;
    faRemove = faSquareMinus;
    size = "grow-6";

    users: User[] = [];
    usersColumns: string[] = [ 'user-add', 'user-edit', 'user-delete', 'user-loginId', 'user-type', 'filler' ];

    devices: RegisteredDevice[] = [];
    devicesColumns: string[] = [ 'device-view', 'device-delete', 'device-deviceId', 'device-loginId', 'device-timestamp', 'device-ipaddress', 'filler' ];

    constructor(
        translate: TranslateService,
        router: Router,
        ngZone: NgZone,
        dialog: MatDialog,
        authenticationService: AuthenticationService,
        private readonly userService: UserService,
        private readonly regDeviceService: RegisteredDeviceService,
        private readonly userTypeService: UserTypeService,
        private readonly popup: MessagePopupService
    ) { 
        super(translate, router, authenticationService, ngZone, dialog);
    }

    protected onInit() {
        this.userService.getAllUsers()
            .pipe(untilDestroyed(this))
            .subscribe(users => this.users = users);

        this.regDeviceService.getAllRegisteredDevices()
            .pipe(untilDestroyed(this))
            .subscribe(devices => this.devices = devices);
    }

    onDelete(loginId: string) {
        MessageBoxComponent.show(this.dialog, {
            title: this.translate.instant('title.confirm'),
            message: this.translate.instant('users.delete-user', { loginId: loginId })
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                this.userService.deleteUser(loginId)
                .pipe(untilDestroyed(this))
                .subscribe(() => this.onInit());
            }
        });
    }

    saveUser(user: User = undefined) {
        UserDialogComponent.showDialog(this.dialog, user)
            .pipe(
                untilDestroyed(this),
                switchMap(user => user?.id === -1 ? of(user) : this.userService.saveUser(user))                
            ).subscribe(user => {
                if (user) {
                    if (user?.id > 0) {
                        this.onInit();
                        this.popup.showMessage('users.success-save', { loginId: user.loginId });
                    }
                } else {
                    this.popup.showError('users.error-save');
                }
            });
    }

    userType(user: User) {
        return this.userTypeService.userTypes.find(ut => ut.id === user?.type)?.description;
    }

    get currentLoginId() {
        const loggedInUser = this.authenticationService.authUserChanged$.getValue();
        return loggedInUser?.loginId;
    }

    onDeleteDevice(deviceId: string) {
        MessageBoxComponent.show(this.dialog, {
            title: this.translate.instant('title.confirm'),
            message: this.translate.instant('users.delete-device', { deviceId : deviceId })
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                this.regDeviceService.deleteRegisteredDevice(deviceId)
                .pipe(untilDestroyed(this))
                .subscribe(() => this.onInit());
            }
        });
    }

    onViewDevice(device: RegisteredDevice) {
        DevicesDialogComponent.showDialog(this.dialog, device)
            .pipe(untilDestroyed(this))
            .subscribe();
    }
}
