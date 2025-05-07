import { Component, OnInit, inject } from '@angular/core';
import { UntypedFormGroup, UntypedFormBuilder } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { UntilDestroy } from '@ngneat/until-destroy';
import { TranslateService } from '@ngx-translate/core';
import { Helper } from 'src/app/helpers/helper';
import { AuthenticationService } from 'src/app/services/api/authentication.services';
import { MenuService } from 'src/app/services/api/menu.service';
import { MessagePopupService } from 'src/app/services/message-popup.service';
import { OverlayService } from 'src/app/services/overlay.service';

@UntilDestroy()
@Component({ selector: 'base-component', template: '' })
export abstract class BaseComponent {
    protected readonly translate = inject(TranslateService);
    protected readonly dialog = inject(MatDialog);
    protected readonly authService = inject(AuthenticationService);
    protected readonly router = inject(Router);   
    protected readonly popup = inject(MessagePopupService);
    protected readonly menuService = inject(MenuService);
    protected readonly overlay = inject(OverlayService);

    protected redirectToDefaultPage() {
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

@UntilDestroy()
@Component({ selector: 'base-form-component', template: '' })
export abstract class BaseFormComponent extends BaseComponent {
    protected formGroup: UntypedFormGroup;
    protected formBuilder = inject(UntypedFormBuilder);

    constructor() {
        super();
        this.formGroup = this.createForm();
    }

    // convenience getter for easy access to form fields
    get f() { return this.formGroup?.controls; }

    protected createForm(): UntypedFormGroup {
        return undefined;
    }

    protected onSubmit() {
        // stop here if form is invalid
        if (this.formGroup?.invalid) {
            return;
        }

        this.onValidFormSubmitted();
    }

    protected onValidFormSubmitted() { }
}


@UntilDestroy()
@Component({ selector: 'base-auth-component', template: '' })
export abstract class BaseAuthComponent extends BaseComponent implements OnInit {
    ngOnInit() {
        if (Helper.isMobile())
            this.router.navigate(['/meteo']); // Forbid Admin mode when using a mobile device

        else if (!this.authService.isUserLoggedIn())
            this.router.navigate(['/login']);
    }
}

@UntilDestroy()
@Component({ selector: 'base-auth-form-component', template: '' })
export abstract class BaseAuthFormComponent extends BaseFormComponent implements OnInit {
    ngOnInit() {
        if (Helper.isMobile())
            this.router.navigate(['/meteo']); // Forbid Admin mode when using a mobile device

        else if (!this.authService.isUserLoggedIn())
            this.router.navigate(['/login']);
    }
}

