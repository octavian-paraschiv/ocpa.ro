import { Component, OnInit } from '@angular/core';
import { UntypedFormGroup, UntypedFormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { TranslateService } from '@ngx-translate/core';
import { first } from 'rxjs/operators';
import { FailedAuthenticationResponse } from 'src/app/models/models-swagger';
import { AuthenticationService } from 'src/app/services/api/authentication.services';
import { MenuService } from 'src/app/services/api/menu.service';

@UntilDestroy()
@Component({
    selector: 'app-mfa',
    templateUrl: './mfa.component.html'
})
export class MfaComponent implements OnInit {
    loginForm: UntypedFormGroup;
    loading = false;
    submitted = false;
    error = '';
    hide = true;

    constructor(
        private translate: TranslateService,
        private formBuilder: UntypedFormBuilder,
        private router: Router,
        private authenticationService: AuthenticationService,
        private menuService: MenuService) { 
        
        if (this.authenticationService.isUserLoggedIn())
            this.redirectToDefaultPage();
    }

    ngOnInit() {
        this.loginForm = this.formBuilder.group({
            mfa: ['', Validators.required],
        });
    }

    // convenience getter for easy access to form fields
    get f() { return this.loginForm.controls; }

    onSubmit() {
        this.submitted = true;

        // stop here if form is invalid
        if (this.loginForm.invalid) {
            return;
        }

        this.loading = true;
        this.authenticationService.sendMfa(this.f.mfa.value)
            .pipe(first(), untilDestroyed(this))
            .subscribe({
                next: msg => (msg?.length > 0) ? this.handleError(msg) : this.redirectToDefaultPage(),
                error: err => this.handleError(err)
            });
    }

    handleError(err: any) {
        const far = err as FailedAuthenticationResponse;
        const username = this.authenticationService.mfaUserChanged$.getValue();
        this.error = far ? 
            this.translate.instant(far.errorMessage, { username, retries: far.loginAttemptsRemaining }) :
            this.translate.instant(err, { username });

        this.loading = false;
    }

    redirectToDefaultPage() {
        setTimeout(() => {
            const defaultPage = 
            (this.menuService?.menus?.appMenus?.length > 0) ? 
                this.menuService.menus.appMenus[0].url :
                (this.menuService?.menus?.publicMenus?.length > 0) ? 
                    this.menuService.menus.publicMenus[0].url : '/';

            this.router.navigate([ defaultPage ]);
        }, 300);
    }
}
