import { Component, OnInit, Input } from '@angular/core';
import { UntypedFormGroup, UntypedFormBuilder, Validators, ValidatorFn, AbstractControl } from '@angular/forms';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { of, Observable, Subject } from 'rxjs';
import { first, map, tap } from 'rxjs/operators';
import { ApplicationInfo, UserInfo } from 'src/app/models/local/access-management';
import { UserType, ApplicationUser, User } from 'src/app/models/swagger/access-management';
import { AppMenuManagementService } from 'src/app/services/api/app-menu-management.service';
import { AuthenticationService } from 'src/app/services/api/authentication.services';
import { UserTypeService } from 'src/app/services/api/user-type.service';
import { SessionInformationService } from 'src/app/services/session-information.service';
import { environment } from 'src/environments/environment';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { faEye, faEyeSlash } from '@fortawesome/free-solid-svg-icons';

@UntilDestroy()
@Component({
  selector: 'user-dialog.component',
  templateUrl: './user-dialog.component.html'
})
export class UserDialogComponent implements OnInit {
  @Input() user: User;

  userForm: UntypedFormGroup;
  hide = true;
  hide2 = true;
  editMode = false;
  userTypes: UserType[] = [];
  apps: ApplicationInfo[] = [];
  appsForUser: ApplicationUser[] = [];
  result$ = new Subject<UserInfo>();
  faEye = faEye;
  faEyeSlash = faEyeSlash;
  size = 'grow-2';

  constructor(
    private sessionInfo: SessionInformationService,
    private appMenuService: AppMenuManagementService,
    private authService: AuthenticationService,
    private userTypeService: UserTypeService,
    private formBuilder: UntypedFormBuilder,
    public bsModalRef: BsModalRef
  ) {
    this.userTypes = this.userTypeService.userTypes;
  }

  ngOnInit(): void {
    this.authService.userLoginState$
      .pipe(untilDestroyed(this))
      .subscribe(() => {
        if (!this.authService.isUserLoggedIn()) this.onCancel();
      });

    this.editMode = !!this.user?.loginId?.length;

    if (!this.user) {
      this.user = {
        loginId: '',
        passwordHash: '',
        type: this.userTypes.find(ut => ut)?.id,
      } as User;
    }

    this.initForm();
    this.loadApps();
  }

  private initForm(): void {
    const isEdit = this.editMode;
    const controls = {
      u1: [this.user.loginId, Validators.required],
      p1: [ '', isEdit ? Validators.minLength(8) : [Validators.required, Validators.minLength(8)] ],
      p2: [ '', this.passwordMatch() ],
      t1: [ this.user.type ],
      email: [ this.user.emailAddress, Validators.email ]
    };

    this.userForm = this.formBuilder.group(controls);

    if (isEdit) this.f.u1.disable();

    const loggedInUser = this.sessionInfo.getUserSessionInformation()?.loginId;
    if (this.user.loginId === loggedInUser) this.f.t1.disable();
  }

  private loadApps(): void {
    const appsForUser$ = this.editMode
      ? this.appMenuService.getAppsForUser(this.user.id).pipe(
          first(),
          tap(apps => this.appsForUser = apps),
          untilDestroyed(this)
        )
      : of([]);

    appsForUser$.subscribe({
      next: () => this.fetchApps(),
      error: () => this.fetchApps()
    });
  }

  private fetchApps(): void {
    this.appMenuService.getAllApps()
      .pipe(first(), untilDestroyed(this))
      .subscribe(apps => {
        this.apps = apps
          .filter(a => a.loginRequired && !a.adminMode)
          .map(a => ({
            ...a,
            selected: !!this.appsForUser?.find(au => au.applicationId === a.id)
          }));
      });
  }

  get f() { return this.userForm?.controls; }

  get title(): string {
    const loggedInUser = this.sessionInfo.getUserSessionInformation()?.loginId;
    return this.editMode
      ? (this.user.loginId === loggedInUser ? 'user-dialog.edit-logged-in' : 'user-dialog.edit')
      : 'user-dialog.create';
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
    this.bsModalRef.hide();
    this.result$.next({ id: -1 } as UserInfo);
    this.result$.complete();
  }

  onOk(): void {
    if (this.userForm.invalid) return;

    this.bsModalRef.hide();
    
    const loginId = this.f.u1.value;
    const pass = this.f.p1.value;
    const type = this.f.t1.value;
    const emailAddress = this.f.email.value;

    const passwordHash = pass?.length > 0
      ? environment.ext.hash(loginId, pass)
      : this.user.passwordHash;

    const result: UserInfo = {
      loginId,
      passwordHash,
      type,
      emailAddress,
      enabled: this.user.enabled,
      useOtp: this.user.useOtp,
      appsForUser: this.apps.filter(a => a.selected).map(a => ({
        applicationId: a.id,
        userId: this.user.id
      }))
    };

    this.result$.next(result);
    this.result$.complete();
  }

  get isApplicationUser(): boolean {
    return this.userTypes.find(ut => ut.id === this.f?.t1?.value)?.code === 'APP';
  }

  static showDialog(modalService: BsModalService, user: User = undefined): Observable<UserInfo> {
    const bsModalRef: BsModalRef<UserDialogComponent> = modalService.show(UserDialogComponent, { 
        initialState: { user },
        class: 'bs-modal'  
    });

    return bsModalRef.content.result$.pipe(map(result => result ?? { id: -1 } as UserInfo));
  }
}
