import { Component, Inject, OnInit } from '@angular/core';
import { AbstractControl, UntypedFormBuilder, UntypedFormGroup, ValidatorFn, Validators } from '@angular/forms';
import { MatChipSelectionChange } from '@angular/material/chips';
import { MatDialog, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Observable, of } from 'rxjs';
import { first, tap, map } from 'rxjs/operators';
import { UserType, User, Application, ApplicationUser } from 'src/app/models/models-swagger';
import { AppMenuManagementService } from 'src/app/services/app-menu-management.service';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { UserTypeService } from 'src/app/services/user-type.service';
import { environment } from 'src/environments/environment';

export interface ApplicationInfo extends Application {
    selected: boolean;
}

export interface UserInfo extends User {
    appsForUser: ApplicationUser[];
}

@UntilDestroy()
@Component({
    selector: 'user-dialog.component',
    templateUrl: './user-dialog.component.html'
})
export class UserDialogComponent implements OnInit {
    userForm: UntypedFormGroup;
    hide = true;
    editMode = false;
    userTypes: UserType[] = undefined;

    apps: ApplicationInfo[] = [];
    appsForUser: ApplicationUser[] = [];

    constructor(
        private appMenuService: AppMenuManagementService,
        private authService: AuthenticationService,
        private userTypeService: UserTypeService,
        private formBuilder: UntypedFormBuilder,
        public dialogRef: MatDialogRef<UserDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public user: User
    ) {
        this.userTypes = this.userTypeService.userTypes;
    }

    ngOnInit() {
        this.authService.authUserChanged$
            .pipe(untilDestroyed(this))
            .subscribe(() => {
                if(!this.authService.isUserLoggedIn())
                    this.onCancel();
            });

        this.editMode = this.user?.loginId?.length > 0;

        if (!this.user)
            this.user = {
                loginId: '',
                passwordHash: '',
                type: this.userTypes.find(ut => ut)?.id,
            } as User;

        if (this.editMode) {
            this.userForm = this.formBuilder.group({
                u1: [ this.user.loginId, Validators.required ],
                p1: [ '', Validators.minLength(8) ],
                p2: [ '', this.passwordMatch() ],
                t1: [ this.user.type ],
                disableAccount: [ !this.user.enabled ],
            });
            this.f.u1.disable();
        } else {
            this.userForm = this.formBuilder.group({
                u1: [ this.user.loginId, Validators.required ],
                p1: [ '', [ Validators.required, Validators.minLength(8)] ],
                p2: [ '', this.passwordMatch() ],
                t1: [ this.user.type ],
                disableAccount: [ !this.user.enabled ],
            });
        }

        const loggedInUser = this.authService.authUserChanged$.getValue();
        if (this.user.loginId === loggedInUser?.loginId)
            this.f.t1.disable();

        const o1 = (this.editMode) ? 
            this.appMenuService.getAppsForUser(this.user.id).pipe(
                first(), 
                tap(appsForUser => this.appsForUser = appsForUser), 
                untilDestroyed(this)) : of([]);

        o1.subscribe({
            next: () => this.fetchApps(),
            error: () => this.fetchApps(),
        });
    }

    fetchApps() {
        this.appMenuService.getAllApps()
            .pipe(first(), untilDestroyed(this))
            .subscribe(apps => this.apps = apps
                .filter(a => a.loginRequired && !a.adminMode)
                .map(a => ({
                    adminMode: a.adminMode,
                    builtin: a.builtin,
                    code: a.code,
                    id: a.id,
                    loginRequired: a.loginRequired,
                    name: a.name,
                    selected: this.appsForUser?.find(au => au.applicationId === a.id)?.id > 0
                } as ApplicationInfo)));
    }

    // convenience getter for easy access to form fields
    get f() { return this.userForm?.controls; }

    get title(): string {
        const loggedInUser = this.authService.authUserChanged$.getValue();
        return (this.editMode) ? 
            (this.user.loginId === loggedInUser?.loginId) ? 
                'user-dialog.edit-logged-in' : 
                'user-dialog.edit' :
            'user-dialog.create';
    }

    passwordMatch(): ValidatorFn {
        return (formGroup: AbstractControl): { [key: string]: any } | null => {
            if (this.f?.p1?.value !== this.f?.p2?.value) {
                this.f?.p2?.setErrors({ unmatched: true });
                return { unmatched: true };
            } else {
                this.f?.p2?.setErrors(null);
                return null;
            }
        };
    }

    onCancel(): void {
        this.dialogRef.close({id: -1} as User);
    }

    onOk(): void {
        // stop here if form is invalid
        if (this.userForm.invalid) {
            return;
        }

        const loginId = this.f?.u1?.value;
        const pass = this.f?.p1?.value;
        const type = this.f?.t1?.value;

        let passwordHash = this.user.passwordHash;
        if (pass?.length > 0) {
            passwordHash =  environment.ext.hash(loginId, pass);
        }

        this.dialogRef.close({ loginId, passwordHash, type, enabled: this.user.enabled,
            appsForUser: this.apps.filter(a => a.selected).map(a => ({
                applicationId: a.id,
                userId: this.user.id,
            } as ApplicationUser)) } as UserInfo);
    }

    get isApplicationUser() {
        return this.userTypes.find(ut => ut.id === this.f?.t1?.value)?.code === 'APP';  
    }

    static showDialog(dialog: MatDialog, user: User = undefined): Observable<UserInfo> {
        const dialogRef = dialog?.open(UserDialogComponent, { data: user, height: 'auto' });
        return dialogRef.afterClosed().pipe(map(result => result as UserInfo));
    }
}
