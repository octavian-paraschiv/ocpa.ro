import { Component, OnInit, Input } from '@angular/core';
import { UntypedFormGroup, UntypedFormBuilder, Validators } from '@angular/forms';
import { UntilDestroy } from '@ngneat/until-destroy';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Observable, Subject } from 'rxjs';
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
    @Input() data: NodeNameDialogData;
    result$: Subject<string> = new Subject<string>();

    constructor(
        private formBuilder: UntypedFormBuilder,
        public bsModalRef: BsModalRef
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
        this.bsModalRef.hide();
        this.result$.next(undefined);
        this.result$.complete();
    }

    onOk(): void {
        // stop here if form is invalid
        if (this.form.invalid) {
            return;
        }

        this.bsModalRef.hide();

        if (this.editMode)
            this.result$.next(`${this.data.node.path}/${this.f.name.value}`);
        else
            this.result$.next(`${this.data.parent.path}/${this.data.parent.name}/${this.f.name.value}`);

        this.result$.complete();
    }

    static showDialog(modalService: BsModalService, data: NodeNameDialogData): Observable<string> {
        const bsModalRef: BsModalRef<NodeNameDialogComponent> = modalService.show(NodeNameDialogComponent, {
                initialState: { data },
                class: 'bs-modal'      
              });
          
        return bsModalRef.content.result$;
    }
}
