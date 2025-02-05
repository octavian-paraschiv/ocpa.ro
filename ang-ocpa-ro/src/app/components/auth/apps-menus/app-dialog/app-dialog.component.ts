import { Component, OnInit, Inject } from '@angular/core';
import { UntypedFormGroup, UntypedFormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { UntilDestroy } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Application } from 'src/app/models/models-swagger';

@UntilDestroy()
@Component({
    selector: 'app-dialog',
    templateUrl: './app-dialog.component.html'
})
export class AppDialogComponent implements OnInit {
    appForm: UntypedFormGroup;
    editMode = false;

    constructor(
        private formBuilder: UntypedFormBuilder,
        public dialogRef: MatDialogRef<AppDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public app: Application
    ) {
    }

    // convenience getter for easy access to form fields
    get f() { return this.appForm?.controls; }

    get title(): string {
        return (this.editMode) ? 
            'app-dialog.edit' :
            'app-dialog.create';
    }


    ngOnInit(){
        this.editMode = this.app?.id > 0;

        if (!this.app)
            this.app = {
                name: '',
                code: '',
                loginRequired: false,
                adminMode: false,
                builtin: false
            } as Application;

        // Should not happen, but anyways
        if (this.app.builtin)
            return;            
        
        this.appForm = this.formBuilder.group({
            name: [ this.app.name, [ Validators.required, Validators.pattern('^[a-zA-Z0-9]{3,16}$') ] ],
            code: [ this.app.code, [ Validators.required, Validators.pattern('^[A-Z]{3,5}$') ] ],
            loginRequired: [ this.app.loginRequired ?? false ],
            adminMode: [ this.app.adminMode ?? false ]
        });
    }

    onCancel(): void {
        this.dialogRef.close();
    }

    onOk(): void {
        // stop here if form is invalid
        if (this.appForm.invalid)
            return;

        this.dialogRef.close({
            id: this.app.id,
            name: this.f?.name?.value,
            code: this.f?.code?.value,
            adminMode: this.f?.adminMode?.value ?? false,
            loginRequired: this.f?.loginRequired?.value ?? false,
            builtin: false
        } as Application);
    }

    static showDialog(dialog: MatDialog, app: Application = undefined): Observable<Application> {
        const dialogRef = dialog?.open(AppDialogComponent, { data: app, width: '500px' });
        return dialogRef.afterClosed().pipe(map(result => (result ?? {id: -1}) as Application));
    }
}