import { BrowserModule } from '@angular/platform-browser';
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { HomeComponent } from './components/non-auth/home/home.component';
import { ProTONEComponent } from './components/non-auth/protone/protone.component';
import { PhotographyComponent } from './components/non-auth/photography/photography.component';
import { ElectronicsComponent } from './components/non-auth/electronics/electronics.component';
import { ContentApiService, GeographyApiService, MeteoApiService, ProtoneApiService } from './services/api-services';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { DaySummaryComponent } from './components/non-auth/meteo/day-summary/day-summary.component';
import { MeteoComponent } from './components/non-auth/meteo/meteo.component';
import { DayDetailsComponent } from './components/non-auth/meteo/day-details/day-details.component';
import { DayRisksComponent } from './components/non-auth/meteo/day-risks/day-risks.component';
import { Helper } from './services/helper';
import { CountryCodePipe, DistancePipe, PressurePipe, SpeedPipe, TempPipe, VolumePipe } from './services/unit-transform-pipe';
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

@NgModule({
    declarations: [
        AppComponent,
        NavMenuComponent,
        HomeComponent,
        ProTONEComponent,
        MeteoComponent,
        MeteoPhotosComponent,
        ElectronicsComponent,
        PhotographyComponent,

        UsersComponent,
        UserDialogComponent,
        MessageBoxComponent,

        MeteoDatabaseComponent,
        LoginComponent,

        DaySummaryComponent,
        DayDetailsComponent,
        DayRisksComponent,
        MeteoDataBrowserComponent,

        TempPipe,
        SpeedPipe,
        DistancePipe,
        VolumePipe,
        PressurePipe,

        CountryCodePipe,
    ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule.forRoot([
          { path: '', component: HomeComponent, pathMatch: 'full', data: { title: 'OcPa\'s Web Site' } },
          { path: 'protone', component: ProTONEComponent, data: { title: 'ProTONE Player Web Site' } },
          { path: 'meteo', component: MeteoComponent, data: { title: 'OcPa\'s Weather Forecast' } },
          { path: 'meteo-photos', component: MeteoPhotosComponent, data: { title: 'OcPa\'s Animated Forecast' } },
          { path: 'photography', component: PhotographyComponent, data: { title: 'OcPa\'s Photo Album' } },
          { path: 'electronics', component: ElectronicsComponent, data: { title: 'OcPa\'s Electronic Blog' } },

          { path: 'login', component: LoginComponent, data: { title: 'Login Module' } },
          
          { path: 'admin', component: UsersComponent, data: { title: 'Manage Users' } },
          { path: 'admin/users', component: UsersComponent, data: { title: 'Manage Users' } },
          { path: 'admin/meteo-database', component: MeteoDatabaseComponent, data: { title: 'Manage Meteo Database' } },
    ]),
    NoopAnimationsModule,
    FontAwesomeModule,
    NgSelectModule,
    MaterialModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    
    AuthenticationService,

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

    ProtoneApiService,
    MeteoApiService,
    UserService,

    ContentApiService,

    Helper
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
