import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ElectronicsComponent } from './components/electronics/electronics.component';
import { MeteoComponent } from './components/meteo/meteo.component';
import { PlayerComponent } from './components/player/player.component';

const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'meteo', component: MeteoComponent },
  { path: 'electronics', component: ElectronicsComponent },
  { path: 'player', component: PlayerComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
