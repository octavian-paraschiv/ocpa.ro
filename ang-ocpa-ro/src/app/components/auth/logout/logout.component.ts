import { Component, OnInit } from '@angular/core';
import { UntypedFormGroup, UntypedFormBuilder, Validators } from '@angular/forms';
import { UntilDestroy } from '@ngneat/until-destroy';
import { AuthenticationService } from 'src/app/services/api/authentication.services';

@UntilDestroy()
@Component({
    selector: 'app-logout',
    templateUrl: './logout.component.html'
})
export class LogoutComponent implements OnInit {
    loginForm: UntypedFormGroup;
    loading = false;
    submitted = false;
    error = '';
    hide = true;

    constructor(
        private formBuilder: UntypedFormBuilder,
        private authenticationService: AuthenticationService) { 
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
        this.authenticationService.logout(true);
    }
}
