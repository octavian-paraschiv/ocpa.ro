import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UntypedFormBuilder, UntypedFormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { UntilDestroy } from '@ngneat/until-destroy';
import { UserType } from 'src/app/models/user';

@UntilDestroy()
@Component({
    selector: 'app-login',
    templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {
    loginForm: UntypedFormGroup;
    loading = false;
    submitted = false;
    error = '';

    constructor(
        private formBuilder: UntypedFormBuilder,
        private router: Router,
        private authenticationService: AuthenticationService
    ) { 
        // redirect to admin if already logged in
        if (this.authenticationService.validAdminUser) { 
            this.router.navigate(['/admin']);
        }
    }

    ngOnInit() {
        this.loginForm = this.formBuilder.group({
            username: ['', Validators.required],
            password: ['', Validators.required]
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
        this.authenticationService.authenticate(this.f.username.value, this.f.password.value, UserType.Admin)
            .pipe(first())
            .subscribe({
                next: msg => (msg?.length > 0) ? this.handleError(msg) : this.router.navigate(['/admin']),
                error: () => this.handleError('Incorrect user name or password.')
            });
    }

    handleError(err: any) {
        this.error = err;
        this.loading = false;
        this.f.password.reset();
    }
}
