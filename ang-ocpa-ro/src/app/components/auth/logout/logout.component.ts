import { Component } from '@angular/core';
import { UntypedFormGroup, Validators } from '@angular/forms';
import { UntilDestroy } from '@ngneat/until-destroy';
import { BaseAuthFormComponent } from 'src/app/components/base/BaseComponent';

@UntilDestroy()
@Component({
    selector: 'app-logout',
    templateUrl: './logout.component.html'
})
export class LogoutComponent extends BaseAuthFormComponent {
    protected createForm(): UntypedFormGroup {
        return this.formBuilder.group({
            username: ['', Validators.required],
            password: ['', Validators.required]
        });
    }

    onValidFormSubmitted() {
        this.authService.logout(true);
    }
}
