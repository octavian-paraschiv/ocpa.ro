import { Injectable } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';

@UntilDestroy()
@Injectable()
export class TranslationInitService {
    constructor(private readonly translate: TranslateService) {
    }

    public init(): Observable<any> {
        const lang = this.translate.getBrowserLang() ?? 'en';
        this.translate.addLangs(['en', 'ro']);
        this.translate.setDefaultLang('en');
        return this.translate.use(lang);
    }
}