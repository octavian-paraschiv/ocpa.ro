import { Injectable } from '@angular/core';
import { InterpolationParameters, TranslateService } from '@ngx-translate/core';
import { ToasterService, ToastNotificationConfiguration, ToastType } from 'ngx-toaster/src/lib';
import { OverlayService } from 'src/app/services/overlay.service';

@Injectable({ providedIn: 'root' })
export class MessagePopupService {
    
    constructor(private toasterService: ToasterService,
        private translate: TranslateService,
        private overlay: OverlayService
    ) { }

    public showInfo(key: string, interpolateParams?: InterpolationParameters) {
        const msg = this.translate.instant(key, interpolateParams);
        this.overlay.hide();
        this.toasterService.showToastMessage({
            message: msg, 
            autoHide: true,
            displayDuration: 5000,
            toastType: ToastType.INFORMATION,
            showCloseButton: true
        } as ToastNotificationConfiguration);
    }

    public showSuccess(key: string, interpolateParams?: InterpolationParameters) {
        const msg = this.translate.instant(key, interpolateParams);
        this.overlay.hide();
        this.toasterService.showToastMessage({
            message: msg, 
            autoHide: true,
            displayDuration: 5000,
            toastType: ToastType.SUCCESS,
            showCloseButton: true
        } as ToastNotificationConfiguration);
    }

    public showError(key: string, interpolateParams?: InterpolationParameters) {
        const msg = this.translate.instant(key, interpolateParams);
        this.overlay.hide();
        this.toasterService.showToastMessage({
            message: msg, 
            autoHide: true,
            displayDuration: 5000,
            toastType: ToastType.ERROR,
            showCloseButton: true
        } as ToastNotificationConfiguration);
    }
}