import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';
import { TranslateLoader, TranslateService } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export const translationConfig = {
    defaultLanguage: 'en',
    loader: {
        provide: TranslateLoader,
        useFactory: (httpClient: HttpClient) => new TranslateHttpLoader(httpClient, environment.translationUrl, '.json'),
        deps: [HttpClient]
    }
}

@UntilDestroy()
@Injectable()
export class TranslationInitService {
    private readonly translate = inject(TranslateService);
    private readonly http = inject(HttpClient);

    public init(): Observable<any> {
        const url = `${environment.translationUrl}_languages.json`;
        const obs = this.http.get<string[]>(url)
        obs.subscribe(langs => {
            const browserLang = this.translate.getBrowserLang() ?? 'en';
            const defaultLang = langs.includes(browserLang) ? browserLang : 'en';
            this.translate.addLangs(langs);
            this.translate.setDefaultLang('en');
            this.translate.use(defaultLang);
        });
        return obs;
    }
}