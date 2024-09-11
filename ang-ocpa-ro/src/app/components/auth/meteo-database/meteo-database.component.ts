import { Component, NgZone, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { BaseAuthComponent } from 'src/app/components/auth/base/BaseAuthComponent';
import { MeteoApiService } from 'src/app/services/api-services';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { take } from 'rxjs/operators';
import { MeteoDbInfo } from 'src/app/models/models-swagger';
import { faEye, faSquareMinus, faUpload, faUpRightFromSquare } from '@fortawesome/free-solid-svg-icons';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { MessageBoxComponent, MessageBoxOptions } from 'src/app/components/shared/message-box/message-box.component';
import { MeteoDataBrowserComponent } from 'src/app/components/non-auth/meteo/meteo-data-browser/meteo-data-browser.component';
import { formatDate } from '@angular/common';

@UntilDestroy()
@Component({
    selector: 'app-meteo-database',
    templateUrl: './meteo-database.component.html'
})
export class MeteoDatabaseComponent extends BaseAuthComponent {
    @ViewChild('meteoDataBrowser', { static: true }) dataBrowser: MeteoDataBrowserComponent;

    faEye = faEye;
    faPromote = faUpRightFromSquare;
    faUpload = faUpload;
    faRemove = faSquareMinus;
    size = "grow-4";

    selectedDatabase: MeteoDbInfo = {};
    databases: MeteoDbInfo[] = [];
    displayedColumns: string[] = [ 
        'db-view', 'db-upload', 'db-promote', 
        'db-name', 'db-status', 'db-range', 'db-length', 
        'db-filler' ];

    studioDownloadUrl: string = undefined;

    constructor(
        router: Router,
        authenticationService: AuthenticationService,
        ngZone: NgZone,
        private readonly meteoApi: MeteoApiService,
        private readonly snackBar: MatSnackBar,
        private readonly dialog: MatDialog
    ) { 
        super(router, authenticationService, ngZone);
    }

    protected onInit(): void {
        this.meteoApi.getStudioDownloadUrl()
            .pipe(take(1), untilDestroyed(this))
            .subscribe(url => this.studioDownloadUrl = url);

        this.meteoApi.getDatabases()
            .pipe(take(1), untilDestroyed(this))
            .subscribe(res => this.databases = res);
    }

    upload(db: MeteoDbInfo) {
        MessageBoxComponent.show(this.dialog, {
            title: 'Confirm',
            message: `Are you sure you want to upload a new file for <b>${db.name}</b>?<br><br>
            If you proceed, the existing <b>${db.name}</b> database will be overwritten,<br>
            without any possibility to recover its current contents.`
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                this.fileOpen((data: ArrayBuffer) => {
                    this.meteoApi.upload(db.dbi, data)
                        .pipe(untilDestroyed(this))
                        .subscribe({
                            next: () => this.snackBar.open(`Succesfully uploaded selected file as \`${db.name}\'.`, 
                                undefined, { duration: 5000 }),
                            error: err => this.snackBar.open(`Error while uploading selected file as \`${db.name}\': ${err.toString()}`, 
                                undefined, { duration: 5000 })
                        });
                });
            }
        });
    }

    view(db: MeteoDbInfo) {
        MessageBoxComponent.show(this.dialog, {
            title: 'Confirm',
            message: `Are you sure you want to view <b>${db.name}</b>?`
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                this.selectedDatabase = db;
                this.dataBrowser.initWithParams(db.dbi, false);
            }
        });
    }

    promote(db: MeteoDbInfo) {
        MessageBoxComponent.show(this.dialog, {
            title: 'Confirm',
            message: `Are you sure you want to promote <b>${db.name}</b> as online?<br><br>
            If you proceed, the current ONLINE database will be overwritten,<br>
            without any possibility to recover its current contents.`
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                this.meteoApi.promote(db.dbi)
                    .pipe(untilDestroyed(this))
                    .subscribe({
                        next: () => this.snackBar.open(`Database \`${db.name}\' succesfully promoted to online.`, 
                            undefined, { duration: 5000 }),
                        error: err => this.snackBar.open(`Error while promoting \`${db.name}\': ${err.toString()}`, 
                            undefined, { duration: 5000 })
                    });
            }
        });
    }

    range(db: MeteoDbInfo): string {
        const start = formatDate(db.calendarRange.start, 'yyyy-MM-dd', 'en-US');
        const end = formatDate(db.calendarRange.end, 'yyyy-MM-dd', 'en-US');
        return `${start} -> ${end}`;
    }

    private fileOpen(callback: (ArrayBuffer) => void) {
        var input = document.createElement('input');
        input.type = 'file';
        input.accept = ".db3";
        input.onchange = function () {
            input.files[0].arrayBuffer().then(callback);
        }
        input.click();
    }
}
