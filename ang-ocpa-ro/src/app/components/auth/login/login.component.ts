import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UntypedFormBuilder, UntypedFormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { UntilDestroy } from '@ngneat/until-destroy';
import { UserTypeService } from 'src/app/services/user-type.service';
import { MenuService } from 'src/app/services/menu.service';

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
    hide = true;

    constructor(
        private readonly userTypeService: UserTypeService,
        private formBuilder: UntypedFormBuilder,
        private router: Router,
        private authenticationService: AuthenticationService,
        private menuService: MenuService) { 
        
        if (this.authenticationService.isUserLoggedIn())
            this.redirectToDefaultPage();
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
        this.authenticationService.authenticate(this.f.username.value, this.f.password.value)
            .pipe(first())
            .subscribe({
                next: msg => (msg?.length > 0) ? this.handleError(msg) : this.redirectToDefaultPage(),
                error: () => this.handleError('Incorrect user name or password.')
            });
    }

    handleError(err: any) {
        this.error = err;
        this.loading = false;
        // this.f.password.reset();
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
