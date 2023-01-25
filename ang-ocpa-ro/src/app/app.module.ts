import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
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
import { DistancePipe, PressurePipe, SpeedPipe, TempPipe, VolumePipe } from './services/unit-transform-pipe';

@NgModule({
    declarations: [
        AppComponent,
        NavMenuComponent,
        HomeComponent,
        ProTONEComponent,
        MeteoComponent,
        ElectronicsComponent,
        PhotographyComponent,

        DaySummaryComponent,
        DayDetailsComponent,
        DayRisksComponent,

        TempPipe,
        SpeedPipe,
        DistancePipe,
        VolumePipe,
        PressurePipe
    ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
      RouterModule.forRoot([
          { path: '', component: HomeComponent, pathMatch: 'full', data: { title: 'OcPa\'s Web Site' } },
            { path: 'protone', component: ProTONEComponent, data: { title: 'ProTONE Player Web Site' } },
            { path: 'meteo', component: MeteoComponent, data: { title: 'OcPa\'s Weather Forecast' } },
            { path: 'photography', component: PhotographyComponent, data: { title: 'OcPa\'s Photo Album' } },
            { path: 'electronics', component: ElectronicsComponent, data: { title: 'OcPa\'s Electronic Blog' } }
    ]),
      NoopAnimationsModule
  ],
  providers: [
    ProtoneApiService,
    GeographyApiService,
    MeteoApiService,

    Helper
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }