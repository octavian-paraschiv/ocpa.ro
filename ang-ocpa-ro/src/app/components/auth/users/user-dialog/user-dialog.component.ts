import { Component, Inject, OnInit } from '@angular/core';
import { AbstractControl, UntypedFormBuilder, UntypedFormGroup, ValidatorFn, Validators } from '@angular/forms';
import { MatDialog, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { UserType, User } from 'src/app/models/models-swagger';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { UserTypeService } from 'src/app/services/user-type.service';
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
    userTypes: UserType[] = undefined

    constructor(
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
                t1: [ this.user.type ]
            });
            this.f.u1.disable();
        } else {
            this.userForm = this.formBuilder.group({
                u1: [ this.user.loginId, Validators.required ],
                p1: [ '', [ Validators.required, Validators.minLength(8)] ],
                p2: [ '', this.passwordMatch() ],
                t1: [ this.user.type ]
            });
        }

        const loggedInUser = this.authService.authUserChanged$.getValue();

        if (this.user.loginId === loggedInUser?.loginId)
            this.f.t1.disable();
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

        let passwordHash = this.user.passwordHash;
        if (pass?.length > 0) {
            passwordHash =  environment.ext.hash(loginId, pass);
        }

        this.dialogRef.close({ loginId, passwordHash, type } as User);
    }

    static showDialog(dialog: MatDialog, user: User = undefined): Observable<User> {
        const dialogRef = dialog?.open(UserDialogComponent, { data: user });
        return dialogRef.afterClosed().pipe(map(result => result as User));
    }
}
