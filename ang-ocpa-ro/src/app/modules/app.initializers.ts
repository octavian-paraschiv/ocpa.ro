import { HTTP_INTERCEPTORS, HttpInterceptor } from '@angular/common/http';
import { APP_INITIALIZER, Provider } from '@angular/core';
import { Observable } from 'rxjs';
import { ErrorInterceptor } from 'src/app/interceptors/error.interceptor';
import { RequestInterceptor } from 'src/app/interceptors/request.interceptor';
import { GeographyApiService } from 'src/app/services/api/geography-api.service';
import { MenuService } from 'src/app/services/api/menu.service';
import { UserTypeService } from 'src/app/services/api/user-type.service';
import { FingerprintService } from 'src/app/services/fingerprint.service';
import { Iso3166HelperService } from 'src/app/services/iso3166-helper.service';
import { TranslationInitService } from 'src/app/services/translation-init.service';

interface ServiceInit {
    init(): Observable<unknown>;
}

function provideAppInitializer<T extends ServiceInit>(service: new (...args: any[]) => T): Provider {
    return {
        provide: APP_INITIALIZER,
        useFactory: (t: T) => () => t.init(),
        deps: [service],
        multi: true
    } as Provider;
}

function provideHttpInterceptor<T extends HttpInterceptor>(service: new (...args: any[]) => T): Provider {
    return { 
        provide: HTTP_INTERCEPTORS, 
        useClass: service, 
        multi: true 
    } as Provider;
}

export const appInitializers = [
    provideHttpInterceptor(RequestInterceptor),
    provideHttpInterceptor(ErrorInterceptor),    

    provideAppInitializer(FingerprintService),
    provideAppInitializer(TranslationInitService),
    provideAppInitializer(GeographyApiService),
    provideAppInitializer(Iso3166HelperService),
    provideAppInitializer(UserTypeService),
    provideAppInitializer(MenuService),
];