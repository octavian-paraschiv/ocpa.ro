import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ElectronicsComponent } from './components/electronics/electronics.component';
import { MeteoComponent } from './components/meteo/meteo.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { PlayerComponent } from './components/player/player.component';
import { HttpClientModule } from '@angular/common/http';
import { CalendarComponent } from './components/calendar/calendar.component';
import { MatGridListModule } from '@angular/material/grid-list';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatOptionModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';

@NgModule({
  declarations: [
    AppComponent,
    DashboardComponent,
    ElectronicsComponent,
    MeteoComponent,
    PlayerComponent,
    CalendarComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    MatGridListModule,
    NoopAnimationsModule,
    MatOptionModule,
    MatSelectModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
