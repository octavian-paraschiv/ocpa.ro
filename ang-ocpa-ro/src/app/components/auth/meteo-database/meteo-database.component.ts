import { formatDate } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { faEye, faUpRightFromSquare, faUpload, faSquareMinus } from '@fortawesome/free-solid-svg-icons';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { take } from 'rxjs/operators';
import { MeteoDatabaseDialogComponent } from 'src/app/components/auth/meteo-database/meteo-database-dialog/meteo-database-dialog.component';
import { BaseAuthComponent } from 'src/app/components/base/BaseComponent';
import { MessageBoxComponent } from 'src/app/components/shared/message-box/message-box.component';
import { MessageBoxOptions } from 'src/app/models/models-local';
import { MeteoDbInfo } from 'src/app/models/models-swagger';
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
    size = "grow-4";

    selectedDatabase: MeteoDbInfo = {};
    databases: MeteoDbInfo[] = [];
    displayedColumns: string[] = [ 
        'db-view', 'db-upload', 'db-promote', 
        'db-name', 'db-status', 'db-range', 'db-length', 
        'db-filler' ];

    studioDownloadUrl: string = undefined;

    ngOnInit(): void {
        this.meteoApi.getStudioDownloadUrl()
            .pipe(take(1), untilDestroyed(this))
            .subscribe(url => this.studioDownloadUrl = url);

        this.meteoApi.getDatabases()
            .pipe(take(1), untilDestroyed(this))
            .subscribe(res => this.databases = res);
    }

    upload(db: MeteoDbInfo) {
        MessageBoxComponent.show(this.dialog, {
            title: this.translate.instant('title.confirm'),
            message: this.translate.instant('meteo-db.upload', { name: db.name })
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
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
