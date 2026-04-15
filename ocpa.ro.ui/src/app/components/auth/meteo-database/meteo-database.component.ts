import { formatDate } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { faEye, faUpRightFromSquare, faUpload, faSquareMinus } from '@fortawesome/free-solid-svg-icons';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { take } from 'rxjs/operators';
import { MeteoDatabaseDialogComponent } from 'src/app/components/auth/meteo-database/meteo-database-dialog/meteo-database-dialog.component';
import { BaseAuthComponent } from 'src/app/components/base/BaseComponent';
import { MessageBoxComponent, MessageBoxOptions } from 'src/app/components/shared/message-box/message-box.component';
import { MeteoDbInfo, MeteoDbStatus } from 'src/app/models/swagger/core-services';
import { MeteoApiService } from 'src/app/services/api/meteo-api.service';

@UntilDestroy()
@Component({
    selector: 'app-meteo-database',
    templateUrl: './meteo-database.component.html'
})
export class MeteoDatabaseComponent extends BaseAuthComponent implements OnInit {
    private readonly meteoApi = inject(MeteoApiService);

    faEye = faEye;
    faPromote = faUpRightFromSquare;
    faUpload = faUpload;
    faRemove = faSquareMinus;
    size = "grow-6";

    MeteoDbStatus = MeteoDbStatus;

    selectedDatabase: MeteoDbInfo = {};
    databases: MeteoDbInfo[] = [];
    displayedColumns: string[] = [ 
        'db-view', 'db-upload',  'db-delete', 'db-promote',
        'db-name', 'db-status', 'db-range', 'db-count'
    ];

    studioDownloadUrl: string = undefined;

    ngOnInit(): void {
        this.meteoApi.getStudioDownloadUrl()
            .pipe(take(1), untilDestroyed(this))
            .subscribe(url => this.studioDownloadUrl = url);

        this.meteoApi.getDatabases()
            .pipe(take(1), untilDestroyed(this))
            .subscribe(res => this.databases = res);
    }

    delete(db: MeteoDbInfo) {
        MessageBoxComponent.show(this.dialog, {
            title: this.translate.instant('title.confirm'),
            message: this.translate.instant('meteo-db.delete', { name: db.name })
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                this.overlay.show();
                this.meteoApi.delete(db.dbi ?? 1)
                    .pipe(untilDestroyed(this))
                    .subscribe({
                        next: () => {
                            this.ngOnInit();
                            this.popup.showSuccess('meteo-db.success-delete', { name: db.name });
                        },
                        error: err => {
                            this.ngOnInit();
                            this.popup.showError('meteo-db.error-delete', {name: db.name, err});
                        }
                    });
            }
        });
    }

    upload(db: MeteoDbInfo) {
        MessageBoxComponent.show(this.dialog, {
            title: this.translate.instant('title.confirm'),
            message: this.translate.instant('meteo-db.upload', { name: db.name })
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                this.overlay.show();
                this.fileOpen((data: ArrayBuffer) => {
                    this.meteoApi.upload(db.dbi ?? 0, data)
                        .pipe(untilDestroyed(this))
                        .subscribe({
                            next: () => {
                                this.ngOnInit();
                                this.popup.showSuccess('meteo-db.success-upload', { name: db.name });
                            },
                            error: err => {
                                this.ngOnInit();
                                this.popup.showError('meteo-db.error-upload', {name: db.name, err});
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
            title: this.translate.instant('title.confirm'),
            message: this.translate.instant('meteo-db.promote', { name: db.name })
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                this.overlay.show();
                this.meteoApi.promote(db.dbi ?? 0)
                    .pipe(untilDestroyed(this))
                    .subscribe({
                        next: () => {
                            this.ngOnInit();
                            this.popup.showSuccess('meteo-db.success-promote', {name: db.name});
                        },
                        error: err => {
                            this.ngOnInit();
                            this.popup.showError('meteo-db.error-promote', {name: db.name, err});
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

    status(db: MeteoDbInfo): string {
        return this.translate.instant(`meteo-db.${db.status}`.toLocaleLowerCase());
    }

    private fileOpen(callback: (ArrayBuffer) => void) {
        var input = document.createElement('input');
        input.type = 'file';
        input.accept = ".db3";
        input.onchange = () => input.files[0].arrayBuffer().then(callback);
        input.onabort = () => this.overlay.hide();
        input.oncancel = () => this.overlay.hide();
        input.onclose = () => this.overlay.hide();
        input.click();
    }
}
