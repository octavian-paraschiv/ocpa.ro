import { Component, ElementRef, inject, ViewChild } from '@angular/core';
import { UntypedFormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
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
    loading = false;
    waiting = false;
    error = '';
    hide = true;
    otpGenerated = false;
    redirectUrl: string = undefined;

    private readonly sessionInfo = inject(SessionInformationService);
    private readonly route = inject(ActivatedRoute);

    @ViewChild('otp') otpInputRef!: ElementRef<HTMLInputElement>;

    ngOnInit() {
        this.route.queryParams.subscribe(params => {
            this.redirectUrl = params['url'];

            if (this.authService.isUserLoggedIn())
                this.performRedirect(this.redirectUrl);
        });

        this.otpInputRef?.nativeElement.focus();
    }

    protected createForm(): UntypedFormGroup {
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
        this.f?.otp?.setValue(undefined);
        this.otpInputRef?.nativeElement.focus();        

        this.authService.generateOtp()
            .pipe(untilDestroyed(this))
            .subscribe(res => {
                this.otpGenerated = res;
                setTimeout(() => this.waiting = false, 60000);
            });
    }

    onSubmit() {
        if (this.formGroup.invalid) return;

        this.loading = true;
        this.authService.validateOtp(this.f.otp.value)
            .pipe(first(), untilDestroyed(this))
            .subscribe({
                next: msg => (msg?.length > 0) ? this.handleError(msg) : this.performRedirect(this.redirectUrl),
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
