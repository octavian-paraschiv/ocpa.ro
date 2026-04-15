import { Injectable } from '@angular/core';
import { InterpolationParameters, TranslateService } from '@ngx-translate/core';
import { IndividualConfig, ToastrService } from 'ngx-toastr';
import { OverlayService } from 'src/app/services/overlay.service';

@Injectable({ providedIn: 'root' })
export class MessagePopupService {

    readonly toastOptions = {
        timeOut: 5000,
        closeButton: true,
        positionClass: 'toast-bottom-right'
    } as IndividualConfig;
    
    constructor(private toasterService: ToastrService,
        private translate: TranslateService,
        private overlay: OverlayService
    ) { }

    public showInfo(key: string, interpolateParams?: InterpolationParameters, toastOptions?: IndividualConfig) {
        const msg = this.translate.instant(key, interpolateParams);
        this.overlay.hide();
        this.toasterService.info(msg, undefined, toastOptions ?? this.toastOptions);
    }

    public showSuccess(key: string, interpolateParams?: InterpolationParameters, toastOptions?: IndividualConfig) {
        const msg = this.translate.instant(key, interpolateParams);
        this.overlay.hide();
        this.toasterService.success(msg, undefined, toastOptions ?? this.toastOptions);
    }

    public showError(key: string, interpolateParams?: InterpolationParameters, toastOptions?: IndividualConfig) {
        const msg = this.translate.instant(key, interpolateParams);
        this.overlay.hide();
        this.toasterService.error(msg, undefined, toastOptions ?? this.toastOptions);
    }
}