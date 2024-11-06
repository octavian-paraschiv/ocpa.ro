import { BrowserModule } from '@angular/platform-browser';
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { ProTONEComponent } from './components/non-auth/protone/protone.component';
import { PhotographyComponent } from './components/non-auth/photography/photography.component';
import { ElectronicsComponent } from './components/non-auth/electronics/electronics.component';
import { ContentApiService, GeographyApiService, MeteoApiService, ProtoneApiService, WikiService } from './services/api-services';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MeteoComponent } from './components/non-auth/meteo/meteo.component';
import { DayRisksComponent } from './components/non-auth/meteo/day-risks/day-risks.component';
import { Helper } from './services/helper';
import { CalendarPipe, CountryCodePipe, DistancePipe, PressurePipe, SpeedPipe, TempPipe, VolumePipe } from './services/unit-transform-pipe';
import { JwtInterceptor } from './helpers/jwt.interceptor';
import { ErrorInterceptor } from './helpers/error.interceptor';
import { UserService } from './services/user.service';
import { AuthenticationService } from './services/authentication.services';
import { LoginComponent } from './components/auth/login/login.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { NgSelectModule } from '@ng-select/ng-select';
import { Iso3166HelperService } from './services/iso3166-helper.service';
import { MeteoPhotosComponent } from 'src/app/components/non-auth/meteo-photos/meteo-photos.component';
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

@NgModule({
    declarations: [
        AppComponent,
        NavMenuComponent,
        
        ProTONEComponent,
        MeteoComponent,
        MeteoPhotosComponent,
        ElectronicsComponent,
        PhotographyComponent,

        UsersComponent,
        UserDialogComponent,
        MessageBoxComponent,

        MeteoDatabaseComponent,
        MeteoDatabaseDialogComponent,

        LoginComponent,
        LogoutComponent,

        DayRisksComponent,
        MeteoDataBrowserComponent,

        WikiViewerComponent,

        TempPipe,
        SpeedPipe,
        DistancePipe,
        VolumePipe,
        PressurePipe,
        CountryCodePipe,
        CalendarPipe,
    ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule.forRoot([
          { path: '', component: MeteoComponent, data: { title: 'OcPa\'s Weather Forecast' } },
          { path: 'meteo', component: MeteoComponent, data: { title: 'OcPa\'s Weather Forecast' } },

          { path: 'protone', component: ProTONEComponent, data: { title: 'ProTONE Player Web Site' } },
          { path: 'meteo-photos', component: MeteoPhotosComponent, data: { title: 'OcPa\'s Animated Forecast' } },
          { path: 'photography', component: PhotographyComponent, data: { title: 'OcPa\'s Photo Album' } },
          { path: 'electronics', component: ElectronicsComponent, data: { title: 'OcPa\'s Electronic Blog' } },

          { path: 'login', component: LoginComponent, data: { title: 'Login' } },
          { path: 'logout', component: LogoutComponent, data: { title: 'Logout' } },
         
          { path: 'admin/users', component: UsersComponent, data: { title: 'Manage Users' } },
          { path: 'admin/meteo-database', component: MeteoDatabaseComponent, data: { title: 'Manage Meteo Database' } },
    ]),
    NoopAnimationsModule,
    FontAwesomeModule,
    NgSelectModule,
    MaterialModule,
    NgChartsModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },

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

    Helper
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
