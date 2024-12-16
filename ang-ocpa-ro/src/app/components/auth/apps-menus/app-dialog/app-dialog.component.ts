import { Component, Inject, OnInit } from '@angular/core';
import { UntypedFormBuilder } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { UntilDestroy } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Application } from 'src/app/models/models-swagger';
import { AppMenuManagementService } from 'src/app/services/app-menu-management.service';

@UntilDestroy()
@Component({
    selector: 'app-dialog',
    templateUrl: './app-dialog.component.html'
})
export class AppDialogComponent implements OnInit {
    constructor(
        private appMenuService: AppMenuManagementService,
        private formBuilder: UntypedFormBuilder,
        public dialogRef: MatDialogRef<AppDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public app: Application
    ) {
    }

    ngOnInit(){
    }

    static showDialog(dialog: MatDialog, app: Application = undefined): Observable<Application> {
        const dialogRef = dialog?.open(AppDialogComponent, { data: app });
        return dialogRef.afterClosed().pipe(map(result => result as Application));
    }
}