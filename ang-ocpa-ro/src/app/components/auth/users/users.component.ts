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
        this.userService.deleteUser(loginId)
            .pipe(untilDestroyed(this))
            .subscribe(() => this.onInit());
    }

    saveUser(user: User = undefined) {
        UserDialogComponent.showDialog(this.dialog, user)
            .pipe(
                untilDestroyed(this),
                switchMap(user => user ? this.userService.saveUser(user) : of(undefined as User))
                
            ).subscribe(user => user ? this.onInit() : console.log(`[${user ? 'MOD' : 'ADD'}] User not saved`));
    }

    userType(user: User) {
        return this.userTypeService.userTypes.find(ut => ut.id === user?.type)?.description;
    }

    get currentLoginId() {
        return this.authenticationService.currentUser?.loginId;
    }
}
