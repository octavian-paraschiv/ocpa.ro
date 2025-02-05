import { Component, OnInit, NgZone } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { faEye, faSquarePlus, faSquarePen, faSquareMinus, faCheck } from '@fortawesome/free-solid-svg-icons';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { TranslateService } from '@ngx-translate/core';
import { of, Observable, throwError } from 'rxjs';
import { first, switchMap, tap } from 'rxjs/operators';
import { BaseAuthComponent } from 'src/app/components/auth/base/BaseAuthComponent';
import { DevicesDialogComponent } from 'src/app/components/auth/users/devices-dialog/devices-dialog.component';
import { UserDialogComponent } from 'src/app/components/auth/users/user-dialog/user-dialog.component';
import { MessageBoxComponent } from 'src/app/components/shared/message-box/message-box.component';
import { MessageBoxOptions, UserInfo } from 'src/app/models/models-local';
import { User, RegisteredDevice } from 'src/app/models/models-swagger';
import { AppMenuManagementService } from 'src/app/services/api/app-menu-management.service';
import { AuthenticationService } from 'src/app/services/api/authentication.services';
import { RegisteredDeviceService } from 'src/app/services/api/registered-device.service';
import { UserTypeService } from 'src/app/services/api/user-type.service';
import { UserService } from 'src/app/services/api/user.service';
import { MessagePopupService } from 'src/app/services/message-popup.service';

@UntilDestroy()
@Component({
    selector: 'app-users',
    templateUrl: './users.component.html'
})
export class UsersComponent extends BaseAuthComponent implements OnInit {
    faEye = faEye;
    faAdd = faSquarePlus;
    faEdit = faSquarePen;
    faRemove = faSquareMinus;
    faCheck = faCheck;
    size = "grow-6";

    users: User[] = [];
    usersColumns: string[] = [ 'user-add', 'user-edit', 'user-delete', 'user-loginId', 'user-type', 'user-disabled', 'filler' ];

    devices: RegisteredDevice[] = [];
    devicesColumns: string[] = [ 'device-view', 'device-delete', 'device-deviceId', 'device-loginId', 'device-timestamp', 'device-ipaddress', 'filler' ];

    constructor(
        translate: TranslateService,
        router: Router,
        ngZone: NgZone,
        dialog: MatDialog,
        authenticationService: AuthenticationService,
        private readonly appMenuService: AppMenuManagementService,
        private readonly userService: UserService,
        private readonly regDeviceService: RegisteredDeviceService,
        private readonly userTypeService: UserTypeService,
        private readonly popup: MessagePopupService
    ) { 
        super(translate, router, authenticationService, ngZone, dialog);
    }

    ngOnInit(): void {
        this.onInit();
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
            message: this.translate.instant('users.delete-user', { loginId })
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                this.userService.deleteUser(loginId)
                .pipe(untilDestroyed(this))
                .subscribe({
                    next: () => {
                        this.onInit();
                        this.popup.showSuccess('users.success-delete', { loginId });
                    },
                    error: err => this.popup.showError(err.toString(), { loginId })
                });
            }
        });
    }

    saveUser(user: User = undefined) {
        UserDialogComponent.showDialog(this.dialog, user).pipe(
            first(),
            untilDestroyed(this),
            switchMap(ui => ui?.id === -1 ? of(ui) : this._saveUser(ui))                
        ).subscribe({
            next: (usr) => {
                if (usr) {
                    if (usr?.id > 0) {
                        this.onInit();
                        this.popup.showSuccess('users.success-save', { loginId: user.loginId });
                    }
                } else {
                    this.popup.showError('users.error-save');
                }
            },
            error: err => this.popup.showError(err.toString(), { loginId: user.loginId })
        });
    }

    _saveUser(ui: UserInfo): Observable<User> {
        return this.userService.saveUser(ui).pipe(
            first(),
            tap(u => {
                if (u?.id > 0) {
                    for (let au of ui.appsForUser)
                        au.userId = u.id;
                    this.appMenuService.saveAppsForUser(u.id, ui.appsForUser)
                        .pipe(first(), untilDestroyed(this))
                        .subscribe({
                            next: () => {},
                            error: err => throwError(err)
                        });
                }
            }),
            untilDestroyed(this));
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
