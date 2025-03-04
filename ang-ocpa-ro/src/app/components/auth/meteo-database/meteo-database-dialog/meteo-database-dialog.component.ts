import { Component, OnInit, ViewChild, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { MeteoDataBrowserComponent } from 'src/app/components/non-auth/meteo/meteo-data-browser/meteo-data-browser.component';
import { AuthenticationService } from 'src/app/services/api/authentication.services';

@UntilDestroy()
@Component({
    selector: 'meteo-database-dialog.component',
    templateUrl: './meteo-database-dialog.component.html'
})
export class MeteoDatabaseDialogComponent implements OnInit {
    @ViewChild('meteoDataBrowser', { static: true }) dataBrowser: MeteoDataBrowserComponent;

    constructor(
        private authService: AuthenticationService,
        public dialogRef: MatDialogRef<MeteoDatabaseDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public dbi: number | undefined
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
        this.dialogRef.close();
    }

    static showDialog(dialog: MatDialog, dbi: number | undefined): Observable<any> {
        const dialogRef = dialog?.open(MeteoDatabaseDialogComponent, 
            { 
                data: dbi,
                panelClass: 'full-screen-dialog'
            });
        return dialogRef.afterClosed();
    }
}
