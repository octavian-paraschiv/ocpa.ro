import { BrowserModule } from '@angular/platform-browser';
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { HomeComponent } from './components/home/home.component';
import { ProTONEComponent } from './components/protone/protone.component';
import { PhotographyComponent } from './components/photography/photography.component';
import { ElectronicsComponent } from './components/electronics/electronics.component';
import { GeographyApiService, MeteoApiService, ProtoneApiService } from './services/api-services';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { DaySummaryComponent } from './components/meteo/day-summary/day-summary.component';
import { MeteoComponent } from './components/meteo/meteo.component';
import { DayDetailsComponent } from './components/meteo/day-details/day-details.component';
import { DayRisksComponent } from './components/meteo/day-risks/day-risks.component';
import { Helper } from './services/helper';
import { CountryCodePipe, DistancePipe, PressurePipe, SpeedPipe, TempPipe, VolumePipe } from './services/unit-transform-pipe';
import { AdminComponent } from './components/admin/admin.component';
import { JwtInterceptor } from './helpers/jwt.interceptor';
import { ErrorInterceptor } from './helpers/error.interceptor';
import { UserService } from './services/user.service';
import { AuthenticationService } from './services/authentication.services';
import { LoginComponent } from './components/login/login.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { NgSelectModule } from '@ng-select/ng-select';
import { Iso3166HelperService } from './services/iso3166-helper.service';
import { MeteoPhotosComponent } from 'src/app/components/meteo-photos/meteo-photos.component';

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

        AdminComponent,
        LoginComponent,

        DaySummaryComponent,
        DayDetailsComponent,
        DayRisksComponent,

        TempPipe,
        SpeedPipe,
        DistancePipe,
        VolumePipe,
        PressurePipe,

        CountryCodePipe
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
          { path: 'admin', component: AdminComponent, data: { title: 'Administration Module' } },
          { path: 'login', component: LoginComponent, data: { title: 'Administration Module' } },
    ]),
    NoopAnimationsModule,
    FontAwesomeModule,
    NgSelectModule
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

    ProtoneApiService,
    MeteoApiService,
    UserService,
    AuthenticationService,
    Helper
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
