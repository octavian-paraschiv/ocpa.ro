import { Component, NgZone } from '@angular/core';
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
import { formatDate } from '@angular/common';
import { MeteoDatabaseDialogComponent } from 'src/app/components/auth/meteo-database/meteo-database-dialog/meteo-database-dialog.component';
import { TranslateService } from '@ngx-translate/core';

@UntilDestroy()
@Component({
    selector: 'app-meteo-database',
    templateUrl: './meteo-database.component.html'
})
export class MeteoDatabaseComponent extends BaseAuthComponent {
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
        translate: TranslateService,
        router: Router,
        ngZone: NgZone,
        authenticationService: AuthenticationService,
        dialog: MatDialog,
        private readonly meteoApi: MeteoApiService,
        private readonly snackBar: MatSnackBar
    ) { 
        super(translate, router, authenticationService, ngZone, dialog);
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
            title: this.translate.instant('confirmation.title'),
            message: this.translate.instant('confirmation.upload-meteo-db', { name: db.name })
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                this.fileOpen((data: ArrayBuffer) => {
                    this.meteoApi.upload(db.dbi ?? 0, data)
                        .pipe(untilDestroyed(this))
                        .subscribe({
                            next: () => {
                                this.snackBar.open(
                                    this.translate.instant('message.success-upload', { name: db.name }), 
                                    undefined, { duration: 5000 });
                                this.onInit();
                            },
                            error: err => {
                                this.snackBar.open(
                                    this.translate.instant('message.error-upload', {name: db.name, err}), 
                                    undefined, { duration: 5000 });
                                    this.onInit();
                            }
                        });
                });
            }
        });
    }

    view(db: MeteoDbInfo) {
        this.selectedDatabase = db;
        MeteoDatabaseDialogComponent.showDialog(this.dialog, db.dbi)
            .pipe(untilDestroyed(this))
            .subscribe();
}

    promote(db: MeteoDbInfo) {
        MessageBoxComponent.show(this.dialog, {
            title: this.translate.instant('confirmation.title'),
            message: this.translate.instant('confirmation.promote-meteo-db', { name: db.name })
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                this.meteoApi.promote(db.dbi ?? 0)
                    .pipe(untilDestroyed(this))
                    .subscribe({
                        next: () => {
                            this.snackBar.open(
                                this.translate.instant('message.success-promote', {name: db.name}), 
                                undefined, { duration: 5000 });
                            this.onInit();
                        },
                        error: err => {
                            this.snackBar.open(
                                this.translate.instant('message.error-promote', {name: db.name, err}), 
                                undefined, { duration: 5000 });
                                this.onInit();
                        }
                    });
            }
        });
    }

    range(db: MeteoDbInfo): string {
        
        const start = db?.calendarRange?.start ? 
            formatDate(db.calendarRange.start, 'yyyy-MM-dd', 'en-US') : 'n/a';

        const end = db?.calendarRange?.end ? 
            formatDate(db.calendarRange.end, 'yyyy-MM-dd', 'en-US') : 'n/a';

        if (end !== 'n/a' && start !== 'n/a')
            return `${start} -> ${end}`;

        return 'n/a';
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
