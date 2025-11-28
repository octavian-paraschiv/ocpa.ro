import { Component, OnInit, ViewChild } from '@angular/core';
import { UntypedFormGroup, UntypedFormBuilder, Validators } from '@angular/forms';
import { UntilDestroy } from '@ngneat/until-destroy';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Observable, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { CustomCheckboxComponent } from 'src/app/components/shared/custom-checkbox/custom-checkbox.component';
import { Application } from 'src/app/models/swagger/access-management';

@UntilDestroy()
@Component({
  selector: 'app-dialog',
  templateUrl: './app-dialog.component.html'
})
export class AppDialogComponent implements OnInit {
  appForm: UntypedFormGroup;
  editMode = false;
  app: Application;
  result$: Subject<Application> = new Subject<Application>();

  @ViewChild('loginRequired', { static: true }) loginRequired: CustomCheckboxComponent;
  @ViewChild('adminMode', { static: true }) adminMode: CustomCheckboxComponent;

  constructor(
    private formBuilder: UntypedFormBuilder,
    public bsModalRef: BsModalRef
  ) {}

  get f() {
    return this.appForm?.controls;
  }

  get title(): string {
    return this.editMode ? 'app-dialog.edit' : 'app-dialog.create';
  }

  ngOnInit(): void {
    this.editMode = this.app?.id > 0;

    if (!this.app) {
      this.app = {
        name: '',
        code: '',
        loginRequired: false,
        adminMode: false,
        builtin: false
      } as Application;
    }

    if (this.app.builtin) return;

    this.appForm = this.formBuilder.group({
      name: [this.app.name, [Validators.required, Validators.pattern('^[a-zA-Z0-9]{3,16}$')]],
      code: [this.app.code, [Validators.required, Validators.pattern('^[A-Z]{3,5}$')]]
    });

    this.loginRequired.checked = this.app.loginRequired;
    this.adminMode.checked = this.app.adminMode;
  }

  onCancel(): void {
    this.bsModalRef.hide();
    this.result$.next({ id: -1 } as Application);
    this.result$.complete();
  }

  onOk(): void {
    if (this.appForm.invalid) return;

    this.bsModalRef.hide();
    this.result$.next({
      id: this.app.id,
      name: this.f?.name?.value,
      code: this.f?.code?.value,
      adminMode: this.adminMode.checked,
      loginRequired: this.loginRequired.checked,
      builtin: false
    } as Application);
    this.result$.complete();
  }

  static showDialog(modalService: BsModalService, app?: Application): Observable<Application> {
    const bsModalRef: BsModalRef<AppDialogComponent> = modalService.show(AppDialogComponent, {
      initialState: { app },
      class: 'bs-modal'      
    });

    return bsModalRef.content.result$.pipe(map(result => result ?? { id: -1 } as Application));
  }
}