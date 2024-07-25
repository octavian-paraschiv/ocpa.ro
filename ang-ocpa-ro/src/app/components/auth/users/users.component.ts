import { Component, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { BaseAuthComponent } from 'src/app/components/auth/base/BaseAuthComponent';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { UserService } from 'src/app/services/user.service';
import { User } from 'src/app/models/user';
import { faSquarePlus, faSquarePen, faSquareMinus } from '@fortawesome/free-solid-svg-icons';
import { MatDialog } from '@angular/material/dialog';
import { UserDialogComponent } from 'src/app/components/auth/users/user-dialog/user-dialog.component';
import { switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { UserTypeService } from 'src/app/services/user-type.service';
import { MessageBoxComponent, MessageBoxOptions } from 'src/app/components/shared/message-box/message-box.component';
import { MatSnackBar } from '@angular/material/snack-bar';

@UntilDestroy()
@Component({
    selector: 'app-users',
    templateUrl: './users.component.html'
})
export class UsersComponent extends BaseAuthComponent {
    faAdd = faSquarePlus;
    faEdit = faSquarePen;
    faRemove = faSquareMinus;
    size = "grow-6";

    users: User[] = [];
    displayedColumns: string[] = [ 'user-add', 'user-edit', 'user-delete', 'user-loginId', 'user-type', 'filler' ];

    constructor(
        router: Router,
        ngZone: NgZone,
        authenticationService: AuthenticationService,
        private readonly snackBar: MatSnackBar,
        private readonly userService: UserService,
        private readonly userTypeService: UserTypeService,
        private readonly dialog: MatDialog
    ) { 
        super(router, authenticationService, ngZone);
    }

    protected onInit() {
        this.userService.getAllUsers()
            .pipe(untilDestroyed(this))
            .subscribe(users => this.users = users);
    }

    onDelete(loginId: string) {
        MessageBoxComponent.show(this.dialog, {
            title: 'Confirm',
            message: `Are you sure you want to delete user: <b>${loginId}</b>?`
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
                    this.snackBar.open(`User \`${user.loginId}\' succesfully saved.`, 
                        undefined, { duration: 5000 });
                } else {
                    this.snackBar.open(`Failed to save user ${user.loginId ?? ''}`.trimEnd(), 
                        undefined, { duration: 5000 });
                }
            });
    }

    userType(user: User) {
        return this.userTypeService.userTypes.find(ut => ut.id === user?.type)?.description;
    }

    get currentLoginId() {
        return this.authenticationService.currentUser?.loginId;
    }
}
