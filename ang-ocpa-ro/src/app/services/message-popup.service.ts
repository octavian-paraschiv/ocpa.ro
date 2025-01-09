import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { InterpolationParameters, TranslateService } from '@ngx-translate/core';

@Injectable({ providedIn: 'root' })
export class MessagePopupService {
    constructor(private snackBar: MatSnackBar,
        private translate: TranslateService
    ) { }

    public showMessage(key: string, interpolateParams?: InterpolationParameters) {
        const msg = this.translate.instant(key, interpolateParams);
        return this.snackBar.open(msg, undefined, { duration: 5000 });
    }

    public showError(key: string, interpolateParams?: InterpolationParameters) {
        const msg = this.translate.instant(key, interpolateParams);
        return this.snackBar.open(msg, undefined, { duration: 5000, panelClass: [ 'error-snackbar' ] });
    }
}