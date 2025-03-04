import { Component, OnInit } from '@angular/core';
import { UntypedFormGroup, Validators } from '@angular/forms';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { first } from 'rxjs/operators';
import { BaseFormComponent } from 'src/app/components/base/BaseComponent';
import { FailedAuthenticationResponse } from 'src/app/models/models-swagger';

@UntilDestroy()
@Component({
    selector: 'app-login',
    templateUrl: './login.component.html'
})
export class LoginComponent extends BaseFormComponent implements OnInit {
    loading = false;
    error = '';
    hide = true;

    ngOnInit() {
        if (this.authService.isUserLoggedIn())
            this.redirectToDefaultPage();
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
                next: msg => (msg?.length > 0) ? this.handleError(msg) : this.redirectToDefaultPage(),
                error: err => this.handleError(err)
            });
    }

    handleError(err: any) {
        if (err === 'auth.sendOTP') {
            // redirect to otp page
            setTimeout(() => this.router.navigate([ '/otp' ]), 300);
            
        } else {
            const far = err as FailedAuthenticationResponse;
            this.error = far ? 
                this.translate.instant(far.errorMessage, { username: this.f.username.value, retries: far.loginAttemptsRemaining }) :
                this.translate.instant(err, { username: this.f.username.value });
        }

        this.loading = false;
    }
}
