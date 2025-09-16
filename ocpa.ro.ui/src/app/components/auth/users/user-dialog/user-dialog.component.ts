import { Component, OnInit, Inject } from '@angular/core';
import { UntypedFormGroup, UntypedFormBuilder, Validators, ValidatorFn, AbstractControl } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { of, Observable } from 'rxjs';
import { first, tap, map } from 'rxjs/operators';
import { ApplicationInfo, UserInfo } from 'src/app/models/models-local';
import { UserType, ApplicationUser, User } from 'src/app/models/models-swagger';
import { AppMenuManagementService } from 'src/app/services/api/app-menu-management.service';
import { AuthenticationService } from 'src/app/services/api/authentication.services';
import { UserTypeService } from 'src/app/services/api/user-type.service';
import { SessionInformationService } from 'src/app/services/session-information.service';
import { environment } from 'src/environments/environment';

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
        private sessionInfo: SessionInformationService,
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
        this.authService.userLoginState$
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
                email: [ this.user.emailAddress, Validators.email ],
            });
            this.f.u1.disable();
        } else {
            this.userForm = this.formBuilder.group({
                u1: [ this.user.loginId, Validators.required ],
                p1: [ '', [ Validators.required, Validators.minLength(8)] ],
                p2: [ '', this.passwordMatch() ],
                t1: [ this.user.type ],
                email: [ this.user.emailAddress, Validators.email ],
            });
        }

        const loggedInUser = this.sessionInfo.getUserSessionInformation()?.loginId;
        if (this.user.loginId === loggedInUser)
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
        const loggedInUser = this.sessionInfo.getUserSessionInformation()?.loginId;
        return (this.editMode) ? 
            (this.user.loginId === loggedInUser) ? 
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
        this.dialogRef.close();
    }

    onOk(): void {
        // stop here if form is invalid
        if (this.userForm.invalid) {
            return;
        }

        const loginId = this.f?.u1?.value;
        const pass = this.f?.p1?.value;
        const type = this.f?.t1?.value;
        const emailAddress = this.f?.email?.value;

        let passwordHash = this.user.passwordHash;
        if (pass?.length > 0) {
            passwordHash =  environment.ext.hash(loginId, pass);
        }

        this.dialogRef.close({ 
            loginId, 
            passwordHash, 
            type, 
            emailAddress,
            enabled: this.user.enabled,
            useOTP: this.user.useOtp,
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
        return dialogRef.afterClosed().pipe(map(result => (result ?? {id: -1}) as UserInfo));
    }
}
