import { Component, OnInit, Inject } from '@angular/core';
import { UntypedFormGroup, UntypedFormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { UntilDestroy } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ContentUnit, ContentUnitType } from 'src/app/models/models-swagger';

export interface NodeNameDialogData {
    parent?: ContentUnit;
    node: ContentUnit;
}

@UntilDestroy()
@Component({
    selector: 'node-name-dialog.component',
    templateUrl: './node-name-dialog.component.html'
})
export class NodeNameDialogComponent implements OnInit {
    form: UntypedFormGroup;
    constructor(
        private formBuilder: UntypedFormBuilder,
        public dialogRef: MatDialogRef<NodeNameDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: NodeNameDialogData
    ) {
    }

    ngOnInit() {
        if (!this.data.node.name) {
            this.data.node.name = 
                (this.data.node.type === ContentUnitType.File) ? 'new-file.md' : 'new-folder';
        }

        this.form = this.formBuilder.group({
            name: [ this.data.node.name, [ Validators.required, Validators.pattern('^[a-zA-Z0-9 ._-]{1,64}$') ] ]
        });
    }

    // convenience getter for easy access to form fields
    get f() { return this.form?.controls; }

    get editMode(): boolean {
        return (!this.data.parent && !!this.data.node.name);
    }

    get nameLabel(): string {
        return (this.editMode) ? 'node-name-dialog.rename-as' : 'node-name-dialog.create-as';
    }

    get title(): string {
        return (this.data.node.type === ContentUnitType.File) ? 
            (this.editMode) ? 'node-name-dialog.rename-file' : 'node-name-dialog.new-file' :
            (this.editMode) ? 'node-name-dialog.rename-folder' : 'node-name-dialog.new-folder';
    }

    get titleName(): string {
        return this.editMode ? this.data.node.name : this.data.parent.name;
    }

    onCancel(): void {
        this.dialogRef.close(undefined);
    }

    onOk(): void {
        // stop here if form is invalid
        if (this.form.invalid) {
            return;
        }

        if (this.editMode)
            this.dialogRef.close(`${this.data.node.path}/${this.f.name.value}`);
        else
            this.dialogRef.close(`${this.data.parent.path}/${this.data.parent.name}/${this.f.name.value}`);
    }

    static showDialog(dialog: MatDialog, data: NodeNameDialogData): Observable<string> {
        const dialogRef = dialog?.open(NodeNameDialogComponent, { data });
        return dialogRef.afterClosed().pipe(map(result => result as string));
    }
}
