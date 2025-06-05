import { Component, OnInit, Inject } from '@angular/core';
import { UntypedFormGroup, UntypedFormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { UntilDestroy } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { EMenuDisplayMode, Menu } from 'src/app/models/models-swagger';

@UntilDestroy()
@Component({
    selector: 'menu-dialog  ',
    templateUrl: './menu-dialog.component.html'
})
export class MenuDialogComponent implements OnInit {
    menuForm: UntypedFormGroup;
    editMode = false;
    displayModes = Object.keys(EMenuDisplayMode);
    menuIcons = Object.keys(fas);
    allIcons = fas;
    size = "grow-3";
    selIcon: string = undefined;

    filterValue = '';
    filteredIcons = this.menuIcons.slice();

    constructor(
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

    ngOnInit() {
        this.editMode = this.menu?.id > 0;

        if (!this.menu)
            this.menu = {
                name: '',
                url: '',
                menuIcon: this.menuIcons[this.menuIcons.length - 1],
                displayModeId: Object.keys(EMenuDisplayMode).indexOf(EMenuDisplayMode.AlwaysHide)
            } as Menu;

        // Should not happen, but anyways
        if (this.menu.builtin)
            return;

        this.selIcon = this.menu.menuIcon;

        this.menuForm = this.formBuilder.group({
            name: [this.menu.name, [Validators.required, Validators.pattern('^.{3,24}$')]],
            url: [this.menu.url, [Validators.required, Validators.pattern('^\/[a-zA-Z0-9/._-]{2,127}$')]],
            displayMode: [Object.keys(EMenuDisplayMode)[this.menu.displayModeId]],
            menuIcon: [this.selIcon]
        });
    }

    onCancel(): void {
        this.dialogRef.close();
    }

    onOk(): void {
        // stop here if form is invalid
        if (this.menuForm.invalid)
            return;

        this.dialogRef.close({
            id: this.menu.id,
            name: this.f?.name?.value,
            url: this.f?.url?.value,
            displayModeId: Object.keys(EMenuDisplayMode).indexOf(this.f?.displayMode?.value),
            menuIcon: this.f?.menuIcon?.value,
            builtin: false
        } as Menu);
    }

    static showDialog(dialog: MatDialog, menu: Menu = undefined): Observable<Menu> {
        const dialogRef = dialog?.open(MenuDialogComponent, { data: menu, width: '600px' });
        return dialogRef.afterClosed().pipe(map(result => (result ?? { id: -1 }) as Menu));
    }
}