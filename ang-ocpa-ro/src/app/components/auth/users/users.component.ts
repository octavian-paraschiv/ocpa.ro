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
import { MatSnackBar } from '@angular/material/snack-bar';
import { RegisteredDevice, User } from 'src/app/models/models-swagger';
import { RegisteredDeviceService } from 'src/app/services/registered-device.service';
import { DevicesDialogComponent } from 'src/app/components/auth/users/devices-dialog/devices-dialog.component';
import { TranslateService } from '@ngx-translate/core';

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
        private readonly snackBar: MatSnackBar,
        private readonly userService: UserService,
        private readonly regDeviceService: RegisteredDeviceService,
        private readonly userTypeService: UserTypeService
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
            title: 'Confirm', // TRANSLATE
            message: `Are you sure you want to delete user: <b>${loginId}</b>?` // TRANSLATE
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
                switchMap(user => user ? this.userService.saveUser(user) : of(undefined as User))
                
            ).subscribe(user => {
                if (user) {
                    this.onInit();
                    this.snackBar.open(`User \`${user.loginId}\' succesfully saved.`, // TRANSLATE
                        undefined, { duration: 5000 });
                } else {
                    this.snackBar.open('Failed to save user', undefined, { duration: 5000 }); // TRANSLATE
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
            title: 'Confirm', // TRANSLATE
            message: `Are you sure you want to forget device: <b>${deviceId}</b>?` // TRANSLATE
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
