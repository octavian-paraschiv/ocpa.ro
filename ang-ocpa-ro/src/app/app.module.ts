import { BrowserModule } from '@angular/platform-browser';
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { ProTONEComponent } from './components/non-auth/protone/protone.component';
import { ContentApiService, GeographyApiService, MeteoApiService, ProtoneApiService, TranslationInitService, WikiService } from './services/api-services';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MeteoComponent } from './components/non-auth/meteo/meteo.component';
import { DayRisksComponent } from './components/non-auth/meteo/day-risks/day-risks.component';
import { CalendarPipe, CountryCodePipe, DistancePipe, PressurePipe, SpeedPipe, TempPipe, VolumePipe } from './services/unit-transform-pipe';
import { JwtInterceptor } from './helpers/jwt.interceptor';
import { ErrorInterceptor } from './helpers/error.interceptor';
import { UserService } from './services/user.service';
import { AuthenticationService } from './services/authentication.services';
import { LoginComponent } from './components/auth/login/login.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { NgSelectModule } from '@ng-select/ng-select';
import { Iso3166HelperService } from './services/iso3166-helper.service';
import { UsersComponent } from 'src/app/components/auth/users/users.component';
import { MeteoDatabaseComponent } from 'src/app/components/auth/meteo-database/meteo-database.component';
import { MaterialModule } from 'src/app/material.module';
import { UserDialogComponent } from 'src/app/components/auth/users/user-dialog/user-dialog.component';
import { UserTypeService } from 'src/app/services/user-type.service';
import { MessageBoxComponent } from 'src/app/components/shared/message-box/message-box.component';
import { MeteoDataBrowserComponent } from 'src/app/components/non-auth/meteo/meteo-data-browser/meteo-data-browser.component';
import { MenuService } from 'src/app/services/menu.service';
import { LogoutComponent } from 'src/app/components/auth/logout/logout.component';
import { NgChartsModule } from 'ng2-charts';
import { MeteoDatabaseDialogComponent } from 'src/app/components/auth/meteo-database/meteo-database-dialog/meteo-database-dialog.component';
import { WikiViewerComponent } from 'src/app/components/shared/wiki-viewer/wiki-viewer.component';
import { FingerprintService } from 'src/app/services/fingerprint.service';
import { RegisteredDeviceService } from 'src/app/services/registered-device.service';
import { DevicesDialogComponent } from 'src/app/components/auth/users/devices-dialog/devices-dialog.component';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { WikiContainerComponent } from 'src/app/components/shared/wiki-container/wiki-container.component';
import { MenuDialogComponent } from 'src/app/components/auth/apps-menus/menu-dialog/menu-dialog.component';
import { AppDialogComponent } from 'src/app/components/auth/apps-menus/app-dialog/app-dialog.component';
import { AppsMenusComponent } from 'src/app/components/auth/apps-menus/apps-menus.component';
import { AppMenuManagementService } from 'src/app/services/app-menu-management.service';
import { MessagePopupService } from 'src/app/services/message-popup.service';
import { UnavailablePageComponent } from 'src/app/components/non-auth/unavailable-page/unavailable-page.component';
import { routes } from 'src/app/module.routes';

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
