import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { MatDialog, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { MeteoDataBrowserComponent } from 'src/app/components/non-auth/meteo/meteo-data-browser/meteo-data-browser.component';

@Component({
    selector: 'meteo-database-dialog.component',
    templateUrl: './meteo-database-dialog.component.html'
})
export class MeteoDatabaseDialogComponent implements OnInit {
    @ViewChild('meteoDataBrowser', { static: true }) dataBrowser: MeteoDataBrowserComponent;

    constructor(
        public dialogRef: MatDialogRef<MeteoDatabaseDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public dbi: number | undefined
    ) {
    }

    ngOnInit() {
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
