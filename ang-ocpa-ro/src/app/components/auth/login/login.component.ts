import { Component, OnInit, inject } from '@angular/core';
import { UntypedFormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
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
    redirectUrl: string = undefined;

    private readonly route = inject(ActivatedRoute);

    ngOnInit() {
        this.route.queryParams.subscribe(params => {
            this.redirectUrl = params['url'];
            if (this.authService.isUserLoggedIn())
                this.performRedirect(this.redirectUrl);
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
                next: msg => (msg?.length > 0) ? this.handleError(msg) : this.performRedirect(this.redirectUrl),
                error: err => this.handleError(err)
            });
    }

    handleError(err: any) {
        if (err === 'auth.sendOTP') {
            // redirect to otp page
            setTimeout(() => this.router.navigate(['/otp'], { queryParams: { url: this.redirectUrl } }), 300);

        } else {
            const far = err as FailedAuthenticationResponse;
            this.error = far ?
                this.translate.instant(far.errorMessage, { username: this.f.username.value, retries: far.loginAttemptsRemaining }) :
                this.translate.instant(err, { username: this.f.username.value });
        }

        this.loading = false;
    }
}
