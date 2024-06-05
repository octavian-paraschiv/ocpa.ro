import { Component, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { BaseAuthComponent } from 'src/app/components/auth/base/BaseAuthComponent';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { ColDef, DomLayoutType } from 'ag-grid-community'; // Column Definition Type Interface
import { UserService } from 'src/app/services/user.service';
import { User, UserType } from 'src/app/models/user';

@UntilDestroy()
@Component({
    selector: 'app-users',
    templateUrl: './users.component.html'
})
export class UsersComponent extends BaseAuthComponent {
    users: User[] = [];

    colDefs: ColDef[] = [
        { field: "id", headerName: "ID", autoHeaderHeight: true },
        { field: "loginId", headerName: "Login ID", autoHeaderHeight: true },
        { field: "type", headerName: "User Type", autoHeaderHeight: true, valueFormatter: p => UserType[p.value] }
      ];

    constructor(
        router: Router,
        authenticationService: AuthenticationService,
        ngZone: NgZone,
        private readonly userService: UserService
    ) { 
        super(router, authenticationService, ngZone);
    }

    protected onInit() {
        this.userService.getAllUsers()
            .pipe(untilDestroyed(this))
            .subscribe({
                next: users => this.users = users
            });
    }

    get domLayout(): DomLayoutType {
        return 'autoHeight';
    }
}
