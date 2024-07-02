import { Component, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { UntilDestroy } from '@ngneat/until-destroy';
import { BaseAuthComponent } from 'src/app/components/auth/base/BaseAuthComponent';
import { AuthenticationService } from 'src/app/services/authentication.services';

@UntilDestroy()
@Component({
    selector: 'app-meteo-database',
    templateUrl: './meteo-database.component.html'
})
export class MeteoDatabaseComponent extends BaseAuthComponent {

    constructor(
        router: Router,
        authenticationService: AuthenticationService,
        ngZone: NgZone
    ) { 
        super(router, authenticationService, ngZone);
    }
}
