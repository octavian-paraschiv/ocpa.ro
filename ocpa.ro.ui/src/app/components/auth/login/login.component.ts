import { Component, OnInit, inject } from '@angular/core';
import { UntypedFormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { faEye, faEyeSlash } from '@fortawesome/free-solid-svg-icons';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Subject } from 'rxjs';
import { first } from 'rxjs/operators';
import { BaseFormComponent } from 'src/app/components/base/BaseComponent';
import { FailedAuthenticationResponse } from 'src/app/models/swagger/access-management';

@UntilDestroy()
@Component({
  selector: 'app-login',
  templateUrl: './login.component.html'
})
export class LoginComponent extends BaseFormComponent implements OnInit {
  loading = false;
  error: string = undefined;
  hide = true;
  redirectUrl?: string;
  faEye = faEye;
  faEyeSlash = faEyeSlash;
  size = 'grow-2';

  result$ = new Subject<boolean>();
  private readonly route = inject(ActivatedRoute);

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.redirectUrl = params['url'];
      if (this.authService.isUserLoggedIn()) {
        this.performRedirect(this.redirectUrl);
      }
    });
  }

  protected createForm(): UntypedFormGroup {
    return this.formBuilder.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  protected onValidFormSubmitted(): void {
    this.loading = true;

    this.authService.authenticate(this.f.username.value, this.f.password.value, false)
      .pipe(first(), untilDestroyed(this))
      .subscribe({
        next: msg => {
          if (msg?.length > 0) {
            this.handleError(msg);
          } else {
            this.result$.next(true);
            this.performRedirect(this.redirectUrl);
          }
        },
        error: err => this.handleError(err)
      });
  }

  handleError(err: any): void {
    if (err === 'auth.sendOTP') {
      setTimeout(() => {
        this.router.navigate(['/otp'], { queryParams: { url: this.redirectUrl } });
      }, 300);
    } else {
      const far = err as FailedAuthenticationResponse;
      this.error = far
        ? (far.loginAttemptsRemaining > 0
            ? this.translate.instant('ERR_BAD_CREDENTIALS', { retries: far.loginAttemptsRemaining })
            : this.translate.instant('ERR_ACCOUNT_DISABLED'))
        : this.translate.instant('ERR_NO_TOKEN');
    }

    this.loading = false;
    this.result$.next(false);
  }
}
