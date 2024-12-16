import { Component, Inject, OnInit } from '@angular/core';
import { UntypedFormBuilder } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { UntilDestroy } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Menu } from 'src/app/models/models-swagger';
import { AppMenuManagementService } from 'src/app/services/app-menu-management.service';

@UntilDestroy()
@Component({
    selector: 'app-menu',
    templateUrl: './app-menu.component.html'
})
export class MenuDialogComponent implements OnInit {
    constructor(
        private appMenuService: AppMenuManagementService,
        private formBuilder: UntypedFormBuilder,
        public dialogRef: MatDialogRef<MenuDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public menu: Menu
    ) {
    }

    ngOnInit(){
    }

    static showDialog(dialog: MatDialog, menu: Menu = undefined): Observable<Menu> {
        const dialogRef = dialog?.open(MenuDialogComponent, { data: menu });
        return dialogRef.afterClosed().pipe(map(result => result as Menu));
    }
}