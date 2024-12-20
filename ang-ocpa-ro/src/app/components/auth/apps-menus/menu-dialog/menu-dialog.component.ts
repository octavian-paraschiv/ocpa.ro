import { Component, Inject, OnInit } from '@angular/core';
import { UntypedFormBuilder, UntypedFormGroup } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { UntilDestroy } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { EMenuDisplayMode, Menu } from 'src/app/models/models-swagger';
import { AppMenuManagementService } from 'src/app/services/app-menu-management.service';

@UntilDestroy()
@Component({
    selector: 'menu-dialog  ',
    templateUrl: './menu-dialog.component.html'
})
export class MenuDialogComponent implements OnInit {
    menuForm: UntypedFormGroup;
    editMode = false;

    constructor(
        private appMenuService: AppMenuManagementService,
        private formBuilder: UntypedFormBuilder,
        public dialogRef: MatDialogRef<MenuDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public menu: Menu
    ) {
    }

     // convenience getter for easy access to form fields
     get f() { return this.menuForm?.controls; }

     get title(): string {
         return (this.editMode) ? 
             'menu-dialog.edit' :
             'menu-dialog.create';
     }

    ngOnInit(){
        this.editMode = this.menu?.id > 0;

        if (!this.menu)
            this.menu = {
               name: '',
               url: '',
               menuIcon: '',
               displayModeId: Object.keys(EMenuDisplayMode).indexOf(EMenuDisplayMode.AlwaysHide)
            } as Menu;

        // Should not happen, but anyways
        if (this.menu.builtin)
            return;
    }

    onCancel(): void {
        this.dialogRef.close();
    }

    onOk(): void {
        // stop here if form is invalid
        if (this.menuForm.invalid) {
            return;
        }   

        this.dialogRef.close({  } as Menu);
    }

    static showDialog(dialog: MatDialog, menu: Menu = undefined): Observable<Menu> {
        const dialogRef = dialog?.open(MenuDialogComponent, { data: menu });
        return dialogRef.afterClosed().pipe(map(result => result as Menu));
    }
}