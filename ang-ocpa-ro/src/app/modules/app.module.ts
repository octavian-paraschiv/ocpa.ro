import { HttpClient, HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule, APP_INITIALIZER } from '@angular/core';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { NgSelectModule } from '@ng-select/ng-select';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { MathjaxModule } from 'mathjax-angular';
import { NgChartsModule } from 'ng2-charts';
import { AppComponent } from 'src/app/components/app.component';
import { AppDialogComponent } from 'src/app/components/auth/apps-menus/app-dialog/app-dialog.component';
import { AppsMenusComponent } from 'src/app/components/auth/apps-menus/apps-menus.component';
import { MenuDialogComponent } from 'src/app/components/auth/apps-menus/menu-dialog/menu-dialog.component';
import { LoginComponent } from 'src/app/components/auth/login/login.component';
import { LogoutComponent } from 'src/app/components/auth/logout/logout.component';
import { MeteoDatabaseDialogComponent } from 'src/app/components/auth/meteo-database/meteo-database-dialog/meteo-database-dialog.component';
import { MeteoDatabaseComponent } from 'src/app/components/auth/meteo-database/meteo-database.component';
import { MfaComponent } from 'src/app/components/auth/mfa/mfa.component';
import { DevicesDialogComponent } from 'src/app/components/auth/users/devices-dialog/devices-dialog.component';
import { UserDialogComponent } from 'src/app/components/auth/users/user-dialog/user-dialog.component';
import { UsersComponent } from 'src/app/components/auth/users/users.component';
import { NavMenuComponent } from 'src/app/components/nav-menu/nav-menu.component';
import { DayRisksComponent } from 'src/app/components/non-auth/meteo/day-risks/day-risks.component';
import { MeteoDataBrowserComponent } from 'src/app/components/non-auth/meteo/meteo-data-browser/meteo-data-browser.component';
import { MeteoComponent } from 'src/app/components/non-auth/meteo/meteo.component';
import { ProTONEComponent } from 'src/app/components/non-auth/protone/protone.component';
import { UnavailablePageComponent } from 'src/app/components/non-auth/unavailable-page/unavailable-page.component';
import { MessageBoxComponent } from 'src/app/components/shared/message-box/message-box.component';
import { WikiContainerComponent } from 'src/app/components/shared/wiki-container/wiki-container.component';
import { WikiViewerComponent } from 'src/app/components/shared/wiki-viewer/wiki-viewer.component';
import { ErrorInterceptor } from 'src/app/interceptors/error.interceptor';
import { JwtInterceptor } from 'src/app/interceptors/jwt.interceptor';
import { MaterialModule } from 'src/app/modules/material.module';
import { routes } from 'src/app/modules/module.routes';
import { AppMenuManagementService } from 'src/app/services/api/app-menu-management.service';
import { AuthenticationService } from 'src/app/services/api/authentication.services';
import { ContentApiService } from 'src/app/services/api/content-api.service';
import { GeographyApiService } from 'src/app/services/api/geography-api.service';
import { MenuService } from 'src/app/services/api/menu.service';
import { MeteoApiService } from 'src/app/services/api/meteo-api.service';
import { ProtoneApiService } from 'src/app/services/api/protone-api.service';
import { RegisteredDeviceService } from 'src/app/services/api/registered-device.service';
import { UserTypeService } from 'src/app/services/api/user-type.service';
import { UserService } from 'src/app/services/api/user.service';
import { WikiService } from 'src/app/services/api/wiki.service';
import { FingerprintService } from 'src/app/services/fingerprint.service';
import { Iso3166HelperService } from 'src/app/services/iso3166-helper.service';
import { MessagePopupService } from 'src/app/services/message-popup.service';
import { TranslationInitService } from 'src/app/services/translation-init.service';
import { TempPipe, SpeedPipe, DistancePipe, VolumePipe, PressurePipe, CountryCodePipe, CalendarPipe } from 'src/app/services/unit-transform-pipe';


const I18N_CONFIG = {
  defaultLanguage: 'en',
  loader: {
    provide: TranslateLoader,
    useFactory: (httpClient: HttpClient) => new TranslateHttpLoader(httpClient, './assets/i18n/', '.json'),
    deps: [HttpClient]
  }
}

@NgModule({
    declarations: [
        AppComponent,
        NavMenuComponent,
        
        ProTONEComponent,
        MeteoComponent,
        UsersComponent,
        UserDialogComponent,
        DevicesDialogComponent,
        MessageBoxComponent,

        MeteoDatabaseComponent,
        MeteoDatabaseDialogComponent,

        LoginComponent,
        LogoutComponent,
        MfaComponent,
        UnavailablePageComponent,

        DayRisksComponent,
        MeteoDataBrowserComponent,

        WikiViewerComponent,
        WikiContainerComponent,

        AppsMenusComponent,
        AppDialogComponent,
        MenuDialogComponent,

        TempPipe,
        SpeedPipe,
        DistancePipe,
        VolumePipe,
        PressurePipe,
        CountryCodePipe,
        CalendarPipe,
    ],
  imports: [
    BrowserModule,
    MathjaxModule.forRoot({
      config: {
        loader: { load: ['input/tex', 'output/svg'] },
        tex: { inlineMath: [['$', '$'], ['\\(', '\\)']] },
        svg: { fontCache: 'global' }
      },
      src: 'https://cdn.jsdelivr.net/npm/mathjax@3/es5/startup.js'
    }),
    TranslateModule.forRoot(I18N_CONFIG),
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule.forRoot(routes),
    NoopAnimationsModule,
    FontAwesomeModule,
    NgSelectModule,
    MaterialModule,
    NgChartsModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },

    TranslationInitService,
    {
      provide: APP_INITIALIZER,
      useFactory: (svc: TranslationInitService) => () => svc.init().toPromise(),
      deps: [TranslationInitService],
      multi: true
    },

    GeographyApiService,
    {
      provide: APP_INITIALIZER,
      useFactory: (svc: GeographyApiService) => () => svc.init().toPromise(),
      deps: [GeographyApiService],
      multi: true
    },

    Iso3166HelperService,
    {
      provide: APP_INITIALIZER,
      useFactory: (svc: Iso3166HelperService) => () => svc.init().toPromise(),
      deps: [Iso3166HelperService],
      multi: true
    },

    UserTypeService,
    {
      provide: APP_INITIALIZER,
      useFactory: (svc: UserTypeService) => () => svc.init().toPromise(),
      deps: [UserTypeService],
      multi: true
    },

    UserService,
    RegisteredDeviceService,
    AuthenticationService,

    FingerprintService,
    {
      provide: APP_INITIALIZER,
      useFactory: (svc: FingerprintService) => () => svc.init().toPromise(),
      deps: [FingerprintService],
      multi: true
    },

    MenuService,
    {
      provide: APP_INITIALIZER,
      useFactory: (svc: MenuService) => () => svc.init().toPromise(),
      deps: [FingerprintService, MenuService],
      multi: true
    },

    ProtoneApiService,
    MeteoApiService,
    ContentApiService,
    WikiService,
    AppMenuManagementService,
    MessagePopupService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
