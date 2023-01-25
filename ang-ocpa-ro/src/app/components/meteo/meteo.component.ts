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

  feelsLikeMap: { [id: string]: { color: string, tip: string}  } = {};

  queryRegion: string = undefined;
  querySubregion: string = undefined;
  queryCity: string = undefined;
  queryUnit: string = undefined;

  constructor(private readonly geoApi: GeographyApiService,
    private readonly meteoApi: MeteoApiService,
    private readonly route: ActivatedRoute) {
    
      this.feelsLikeMap['normal'] =       { color:'white',      tip: 'Normal temperature' };
      this.feelsLikeMap['warmer'] =       { color:'#ffffcc',    tip: 'Warmer than normal' };
      this.feelsLikeMap['much_warmer'] =  { color:'#ffff66',    tip: 'Much warmer than normal' };
      this.feelsLikeMap['hot'] =          { color:'#ffcc99',    tip: 'Scorching hot' };      
      this.feelsLikeMap['colder'] =       { color:'#e6f7ff',    tip: 'Colder than normal' };
      this.feelsLikeMap['much_colder'] =  { color:'#e6efff',    tip: 'Much colder than normal' };
      this.feelsLikeMap['frost'] =        { color:'#ccdfff',    tip: 'Bitterly cold' };
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

  public getInnerCellStyle(md: MeteoDailyData) {
    let tempFeel = (md.tempFeel && md.tempFeel.length > 0) ? md.tempFeel : 'normal';
    return {
      'background': `${this.feelsLikeMap[tempFeel].color}`,
      'border': '2px solid #EEEEEE',
      'vertical-align': 'top'
    }
  }

  public getOuterCellStyle(md: MeteoDailyData) {
    const today = md.date === this.today;
    let tempFeel = (md.tempFeel && md.tempFeel.length > 0) ? md.tempFeel : 'normal';
    return {
      'border': `${today ? 5 : 4}px solid ${today ? 'teal' : 'transparent'}`,
    }
  }

  public getLabelStyle(md: MeteoDailyData) {
    const day = new Date(md.date).getDay();
    const weekend = (day % 6 === 0);
    return {
      'background': `${weekend ? 'maroon' : 'black'}`,
      'color': 'white',
      'font-size': '0.9rem',
      'font-weight': `${weekend ? 'bold' : '' }`,
      'text-transform': 'uppercase',
      'vertical-align': 'middle',
      'height': '2vh'
    }
  }

  public getTooltip(md: MeteoDailyData) {
    let tempFeel = (md.tempFeel && md.tempFeel.length > 0) ? md.tempFeel : 'normal';
    let tip = '';
    
    tip += `${this.feelsLikeMap[tempFeel].tip}`;
    
    if (md.wind > 0) {
      tip += `\nWind: ${md.wind} kmh from ${md.windDirection}`;
    }

    if (md.snowCover > 300) {
      tip += `\nSnow cover: >300 cm`;
    } else if (md.snowCover > 0) {
      tip += `\nSnow cover: ${md.snowCover} cm`;
    }

    if (md.hazards) {
      md.hazards.forEach(h => {
        tip += `\n${h}`
      })
    }

    return tip;
  }

  public extraDetails(md: MeteoDailyData): string {
    let tip = '';
    
    if (md.wind > 0) {
      tip += `<label class='detail'>&#129050;&nbsp;Wind: ${md.wind} kmh from ${md.windDirection}</label><br>`;
    }

    if (md.snowCover > 300) {
      tip += `<label class='detail'>&#10033;&nbsp;Snow cover: >300 cm<br>`;
    } else if (md.snowCover > 0) {
      tip += `<label class='detail'>&#10033;&nbsp;Snow cover: ${md.snowCover} cm</label><br>`;
    }

    if (md.hazards) {
      md.hazards.forEach(h => {
        tip += `<img src='assets/icons/meteo/warning.png' width="12vw" height="12vh"><label class='hazard'>\n${h}</label><br>`
      })
    }

    return (tip.length > 0) ? tip : undefined;
  }

  public isToday(md: MeteoDailyData): boolean {
    return md?.date === this.today;
  }
}
