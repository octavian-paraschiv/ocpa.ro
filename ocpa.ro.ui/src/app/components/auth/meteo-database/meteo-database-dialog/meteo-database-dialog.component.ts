import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Observable, Subject } from 'rxjs';
import { MeteoDataBrowserComponent } from 'src/app/components/non-auth/meteo/meteo-data-browser/meteo-data-browser.component';
import { AuthenticationService } from 'src/app/services/api/authentication.services';

@UntilDestroy()
@Component({
    selector: 'meteo-database-dialog.component',
    templateUrl: './meteo-database-dialog.component.html'
})
export class MeteoDatabaseDialogComponent implements OnInit {
    @ViewChild('meteoDataBrowser', { static: true }) dataBrowser: MeteoDataBrowserComponent;
    @Input() dbi: number | undefined;
    result$: Subject<any> = new Subject<any>();

    constructor(
        private authService: AuthenticationService,
        public bsModalRef: BsModalRef
    ) {
    }

    ngOnInit() {
        this.authService.userLoginState$
            .pipe(untilDestroyed(this))
            .subscribe(() => {
                if(!this.authService.isUserLoggedIn())
                    this.onClose();
            });

        this.dataBrowser.initWithParams(this.dbi ?? 0, false);
    }

    onClose(): void {
        this.bsModalRef.hide();
        this.result$.complete();
    }

    static showDialog(modalService: BsModalService, dbi: number | undefined): Observable<any> {
        const bsModalRef: BsModalRef<MeteoDatabaseDialogComponent> = modalService.show(MeteoDatabaseDialogComponent, { 
                initialState: { dbi },
                class: 'meteo-db-modal'  
            });
        
        return bsModalRef.content.result$;
    }
}
