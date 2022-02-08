import { Component, OnDestroy, OnInit } from "@angular/core";
import { HttpClient } from '@angular/common/http';
import { UntilDestroy, untilDestroyed } from "@ngneat/until-destroy";
import { environment } from "src/environments/environment";
import { CalendarRange, MeteoData } from "src/app/models/meteo";

@UntilDestroy()
@Component({
  selector: 'app-meteo',
  templateUrl: './meteo.component.html',
  styleUrls: [ './meteo.component.scss' ]
})
export class MeteoComponent
implements OnInit, OnDestroy {

  regions: string[] = null;
  subregions: string[] = null;
  cities: string[] = null;

  region = '';
  subregion = '';
  city = '';

  result: any = null;

  meteoData: MeteoData = null;

  constructor(private http: HttpClient) {
  }

  ngOnInit() {
    this.http
      .get<string[]>(`${environment.apiBaseUrl}/geography`)
      .pipe(untilDestroyed(this))
      .subscribe(
        (regions => {
          this.regions = regions;
          this.region = regions[0];
        })
      );
  }

  ngOnDestroy() {
  }

  public onRegionSelected(event) {
    if (event.source.selected) {
      this.region = event.source.value;
      this.http
      .get<string[]>(`${environment.apiBaseUrl}/geography?region=${this.region}`)
      .pipe(untilDestroyed(this))
      .subscribe(
        (subregions => {
            this.subregions = subregions;
            this.subregion = subregions[0];
        })
      );
    }
 }

  public onSubregionSelected(event) {
    if (event.source.selected) {
      this.subregion = event.source.value;
      this.http
      .get<string[]>(`${environment.apiBaseUrl}/geography?region=${this.region}&subregion=${this.subregion}`)
      .pipe(untilDestroyed(this))
      .subscribe(
        (cities => {
          this.cities = cities;
          this.city = cities[0];
        })
      );
    }
  }

  public onCitySelected(event) {
    if (event.source.selected) {
      this.city = event.source.value;
      this.http
      .get<CalendarRange>(`${environment.apiBaseUrl}/meteo?region=${this.region}&subregion=${this.subregion}&city=${this.city}`)
      .pipe(untilDestroyed(this))
      .subscribe(res =>
        {
          this.http
          .get<MeteoData>(`${environment.apiBaseUrl}/meteo?region=${this.region}&subregion=${this.subregion}&city=${this.city}&skip=0&take=${res.Length}`)
          .pipe(untilDestroyed(this))
          .subscribe(res2 => this.meteoData = res2);
        });
      }
  }
}
