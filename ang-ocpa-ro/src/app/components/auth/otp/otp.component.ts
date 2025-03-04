import { Component, inject } from '@angular/core';
import { UntypedFormGroup, Validators } from '@angular/forms';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { first } from 'rxjs/operators';
import { BaseFormComponent } from 'src/app/components/base/BaseComponent';
import { FailedAuthenticationResponse } from 'src/app/models/models-swagger';
import { SessionInformationService } from 'src/app/services/session-information.service';

@UntilDestroy()
@Component({
    selector: 'app-otp',
    templateUrl: './otp.component.html'
})
export class OtpComponent extends BaseFormComponent {
    loginForm: UntypedFormGroup;
    loading = false;
    waiting = false;
    error = '';
    hide = true;
    otpGenerated = false;

    private sessionInfo = inject(SessionInformationService);

    protected createForm(): UntypedFormGroup {
        if (this.authService.isUserLoggedIn())
            this.redirectToDefaultPage();

        this.onGenerateOtp();

        return this.formBuilder.group({
            otp: ['', Validators.required],
        });
    }

    get anonymizedEmail() {
        return this.sessionInfo.getUserSessionInformation()?.anonymizedEmail;
    }

    onGenerateOtp() {
        this.waiting = true;
        this.authService.generateOtp()
            .pipe(untilDestroyed(this))
            .subscribe(res => {
                this.otpGenerated = res;
                setTimeout(() => this.waiting = false, 60000);
            });
    }

    onValidFormSubmitted() {
        this.loading = true;
        this.authService.validateOtp(this.f.otp.value)
            .pipe(first(), untilDestroyed(this))
            .subscribe({
                next: msg => (msg?.length > 0) ? this.handleError(msg) : this.redirectToDefaultPage(),
                error: err => this.handleError(err)
            });
    }

    handleError(err: any) {
        const far = err as FailedAuthenticationResponse;
        const username = this.sessionInfo.getUserSessionInformation()?.loginId;
        this.error = far ? 
            this.translate.instant(far.errorMessage, { username, retries: far.loginAttemptsRemaining }) :
            this.translate.instant(err, { username });

        this.loading = false;
    }
}
