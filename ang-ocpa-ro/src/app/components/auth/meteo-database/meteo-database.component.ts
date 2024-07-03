import { Component, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { BaseAuthComponent } from 'src/app/components/auth/base/BaseAuthComponent';
import { MeteoApiService } from 'src/app/services/api-services';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { take } from 'rxjs/operators';

@UntilDestroy()
@Component({
    selector: 'app-meteo-database',
    templateUrl: './meteo-database.component.html'
})
export class MeteoDatabaseComponent extends BaseAuthComponent {

    studioDownloadUrl: string = undefined;

    constructor(
        router: Router,
        authenticationService: AuthenticationService,
        ngZone: NgZone,
        private readonly api: MeteoApiService
    ) { 
        super(router, authenticationService, ngZone);
    }

    protected onInit(): void {
        this.api.getStudioDownloadUrl()
        .pipe(take(1), untilDestroyed(this))
        .subscribe(url => this.studioDownloadUrl = url);
    }
}
