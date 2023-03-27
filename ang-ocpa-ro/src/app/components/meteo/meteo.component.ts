import { formatDate } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Subject } from 'rxjs';
import { take, takeUntil, filter } from 'rxjs/operators';
import { City, GridCoordinates } from 'src/app/models/geography';
import { MeteoDailyData, MeteoData } from 'src/app/models/meteo';
import { GeographyApiService, MeteoApiService } from 'src/app/services/api-services';

@UntilDestroy()
@Component({
  selector: 'app-meteo',
    templateUrl: './meteo.component.html',
    styleUrls: ['../../../assets/styles/site.css']
})
export class MeteoComponent  implements OnInit {
  isFetching = false;
  timerId = 0;

  fetchEvent$ = new Subject();
  regionChanged$ = new Subject();
  subregionChanged$ = new Subject();

  regions: string[];
  selRegion: string;

  subregions: string[];
  selSubregion: string;

  cities: string[];
  selCity: string;

  city: City;
  grid: GridCoordinates;

  meteoData: MeteoDailyData[][] = [];

  today: string;
  todayData: MeteoDailyData;

  queryRegion: string = undefined;
  querySubregion: string = undefined;
  queryCity: string = undefined;
  queryUnit: string = undefined;

  constructor(private readonly geoApi: GeographyApiService,
    private readonly meteoApi: MeteoApiService,
    private readonly route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.meteoData = [];
    this.today = new Date().toISOString().slice(0, 10);

    this.route.queryParams
      .pipe(take(1), untilDestroyed(this))
      .subscribe(params => {
        this.queryRegion = params['region'];
        this.querySubregion = params['subregion'];
        this.queryCity = params['city'];
        this.queryUnit = params['unit'];

        this.geoApi.getRegions()
        .pipe(take(1), untilDestroyed(this))
        .subscribe(regions => {
          this.regions = regions;
          if (this.regions?.length > 0) {
            let selRegion = this.regions[0];
            if (this.regions.includes(this.queryRegion ?? '')) {
              selRegion = this.queryRegion;
            }
            this.selRegion = selRegion;            
            this.onRegionChanged();
          }
        });
      });

  }

  public onRegionChanged() {
    this.meteoData = [];
    this.regionChanged$.next();
    this.geoApi.getSubregions(this.selRegion)
      .pipe(takeUntil(this.regionChanged$), take(1), untilDestroyed(this))
      .subscribe(subregions => {
        this.subregions = subregions;
        if (this.subregions?.length > 0) {
          let selSubregion = this.subregions[0];
          if (this.subregions.includes(this.querySubregion ?? '')) {
            selSubregion = this.querySubregion;
          }
          this.selSubregion = selSubregion;
          this.onSubregionChanged();
        }
      });
  }

  public onSubregionChanged() {
    this.meteoData = [];
    this.subregionChanged$.next();
    this.geoApi.getCities(this.selRegion, this.selSubregion)
      .pipe(takeUntil(this.subregionChanged$), take(1), untilDestroyed(this))
      .subscribe(cities => {
        this.cities = cities;
        if (this.cities?.length > 0) {
          let selCity = this.cities[0];
          if (this.cities.includes(this.queryCity ?? '')) {
            selCity = this.queryCity;
          }
          this.selCity = selCity;
          this.onCityChanged();
        }
      });
  }

  public onCityChanged() {
    this.meteoData = [];

    if (this.timerId > 0) {
      clearTimeout(this.timerId);
      this.timerId = 0;
    }

    this.timerId = window.setTimeout(() => {
      clearTimeout(this.timerId);
      this.timerId = 0;
      this.doFetch();
    }, 2000);
  }

  private doFetch() {
    this.isFetching = true;
    this.fetchEvent$.next();

    this.geoApi.getCityInfo(this.selRegion, this.selSubregion, this.selCity)
      .pipe(takeUntil(this.fetchEvent$), take(1), untilDestroyed(this))
      .subscribe(cityInfo => {
        this.city = cityInfo;
        this.meteoData = [];
        this.geoApi.getGridCoordinates(this.selRegion, this.selSubregion, this.selCity)
          .pipe(takeUntil(this.fetchEvent$), take(1), untilDestroyed(this))
          .subscribe(grid => {
            this.grid = grid;
            this.meteoData = [];
            this.meteoApi.getData(this.selRegion, this.selSubregion, this.selCity, 0, 0)
              .pipe(takeUntil(this.fetchEvent$), take(1), untilDestroyed(this))
              .subscribe(meteoApiData => {
                this.processApiData(meteoApiData);
                this.isFetching = false;
              });
          });
      });
  }

  private processApiData(meteoApiData: MeteoData) {
    let mdEx: MeteoDailyData[] = [];
    let md: MeteoDailyData = undefined;

    if (meteoApiData?.data) {
      for(const date of Object.keys(meteoApiData.data)) {
        const data = meteoApiData.data[date];
        data.date = date;
        mdEx.push(data);
      }

      let startDate = new Date(meteoApiData.calendarRange.start);
      let endDate = new Date(meteoApiData.calendarRange.end);

      while(startDate.getDay() != 1 /* Monday */) {
        startDate.setDate(startDate.getDate() - 1);
        md = {
          date: startDate.toISOString().slice(0, 10)
        };
        mdEx.push(md);
      }

      while(endDate.getDay() != 0 /* Sunday */) {
        endDate.setDate(endDate.getDate() + 1);
        md = {
          date: endDate.toISOString().slice(0, 10)
        };
        mdEx.push(md);
      }
    }
    mdEx = mdEx.sort((m1, m2) => m1.date.localeCompare(m2.date));

    for(let i = 0; i < mdEx.length; i++) {
      const r = Math.floor(i / 7);
      const c = i % 7;
      if (c === 0) {
        this.meteoData[r] = [];
      }
      this.meteoData[r][c] = mdEx[i];

      if (mdEx[i].date == '2022-10-30') {
        this.todayData = mdEx[i];
      }
    }
  }
}
