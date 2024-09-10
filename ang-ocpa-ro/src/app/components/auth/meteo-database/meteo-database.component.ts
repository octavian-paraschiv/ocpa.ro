import { AfterViewInit, Component, NgZone, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { BaseAuthComponent } from 'src/app/components/auth/base/BaseAuthComponent';
import { ContentApiService, MeteoApiService } from 'src/app/services/api-services';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { take } from 'rxjs/operators';
import { ContentUnit, ContentUnitType } from 'src/app/models/models-swagger';
import { faEye, faSquareMinus, faSquarePlus, faUpload, faUpRightFromSquare } from '@fortawesome/free-solid-svg-icons';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { MessageBoxComponent, MessageBoxOptions } from 'src/app/components/shared/message-box/message-box.component';
import { MeteoDataBrowserComponent } from 'src/app/components/non-auth/meteo/meteo-data-browser/meteo-data-browser.component';

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

    selectedDatabase: ContentUnit = {};
    databases: ContentUnit[] = [];
    displayedColumns: string[] = [ 'db-view', 'db-upload', 'db-promote', 'db-delete', 'db-name', 'db-filler' ];

    studioDownloadUrl: string = undefined;

    constructor(
        router: Router,
        authenticationService: AuthenticationService,
        ngZone: NgZone,
        private readonly meteoApi: MeteoApiService,
        private readonly contentApi: ContentApiService,
        private readonly snackBar: MatSnackBar,
        private readonly dialog: MatDialog
    ) { 
        super(router, authenticationService, ngZone);
    }

    protected onInit(): void {
        this.meteoApi.getStudioDownloadUrl()
            .pipe(take(1), untilDestroyed(this))
            .subscribe(url => this.studioDownloadUrl = url);

        this.contentApi.getContent('Meteo', 1, '*.db3')
            .pipe(take(1), untilDestroyed(this))
            .subscribe(ct => 
                this.databases = ct.children.filter(c => 
                    c.name !== 'Preview.db3' && c.type === ContentUnitType.File));
    }

    upload(db: ContentUnit) {
        MessageBoxComponent.show(this.dialog, {
            title: 'Confirm',
            message: `Are you sure you want to upload a new file for <b>${db.name}</b>?<br><br>
            If you proceed, the existing <b>${db.name}</b> database will be overwritten,<br>
            without any possibility to recover its current contents.`
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                // todo effective promote
            }
        });
    }

    view(db: ContentUnit) {
        MessageBoxComponent.show(this.dialog, {
            title: 'Confirm',
            message: `Are you sure you want to view <b>${db.name}</b>?`
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                this.selectedDatabase = db;
                this.dataBrowser.initWithParams(this.dbi(), false);
            }
        });
    }

    promote(db: ContentUnit) {
        MessageBoxComponent.show(this.dialog, {
            title: 'Confirm',
            message: `Are you sure you want to promote <b>${db.name}</b> as online?<br><br>
            If you proceed, the current ONLINE database will be overwritten,<br>
            without any possibility to recover its current contents.`
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                // todo effective promote
            }
        });
    }

    delete(db: ContentUnit) {
        MessageBoxComponent.show(this.dialog, {
            title: 'Confirm',
            message: `Are you sure you want to delete <b>${db.name}</b>?<br>
            If your proceed, you will not be able to recover its contents.`
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                // todo effective delete
            }
        });
    }

    dbi(): number {
        return (this.selectedDatabase?.name === 'Snapshot.db3') ? -1 : 
        parseInt(this.selectedDatabase.name.replace('Preview', '').replace('.db3', ''));
    }
}
