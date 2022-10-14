import { formatDate } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { take } from 'rxjs/operators';
import { City, GridCoordinates } from 'src/app/models/geography';
import { MeteoDailyDataEx, MeteoData } from 'src/app/models/meteo';
import { GeographyApiService, MeteoApiService } from 'src/app/services/api-services';

@Component({
  selector: 'app-meteo',
    templateUrl: './meteo.component.html',
    styleUrls: ['../../../assets/styles/site.css']
})
export class MeteoComponent  implements OnInit {
  regions: string[];
  selRegion: string;

  subregions: string[];
  selSubregion: string;

  cities: string[];
  selCity: string;

  city: City;
  grid: GridCoordinates;

  meteoData: MeteoDailyDataEx[][] = [];

  today: string;

  feelsLikeMap: { [id: string]: { color: string, tip: string}  } = {};

  constructor(private readonly geoApi: GeographyApiService,
    private readonly meteoApi: MeteoApiService) {
    
      this.feelsLikeMap['normal'] =       { color:'white',      tip: 'Normal temperature' };

      this.feelsLikeMap['warmer'] =       { color:'#ffffcc',    tip: 'Warmer than normal' };
      this.feelsLikeMap['much_warmer'] =  { color:'#ffff66',    tip: 'Much warmer than normal' };
      this.feelsLikeMap['hot'] =          { color:'#ffcc99',  tip: 'Scorching hot' };
      
      this.feelsLikeMap['colder'] =       { color:'#e6f7ff',  tip: 'Colder than normal' };
      this.feelsLikeMap['much_colder'] =  { color:'#e6efff', tip: 'Much colder than normal' };
      this.feelsLikeMap['frost'] =        { color:'#ccdfff',  tip: 'Bitterly cold' };
      
  }

  ngOnInit(): void {
    this.meteoData = [];
    this.today = new Date().toISOString().slice(0, 10)

    this.geoApi.getRegions()
      .pipe(take(1))
      .subscribe(regions => {
        this.regions = regions;
        if (this.regions && this.regions.length > 0) {
          this.onRegionChanged(this.regions[0]);
        }
      });
  }

  public onRegionChanged(region) {
    this.selRegion = region;
    this.meteoData = [];
    this.geoApi.getSubregions(this.selRegion)
      .pipe(take(1))
      .subscribe(subregions => {
        this.subregions = subregions;
        if (this.subregions && this.subregions.length > 0) {
          this.onSubregionChanged(this.subregions[0]);
        }
      });
  }

  public onSubregionChanged(subregion) {
    this.selSubregion = subregion;
    this.meteoData = [];
    this.geoApi.getCities(this.selRegion, this.selSubregion)
      .pipe(take(1))
      .subscribe(cities => {
        this.cities = cities;
        if (this.cities && this.cities.length > 0) {
          this.onCityChanged(this.cities[0]);
        }
      });
  }

  public onCityChanged(city) {
    this.selCity = city;
    this.meteoData = [];

    this.geoApi.getCityInfo(this.selRegion, this.selSubregion, this.selCity)
      .pipe(take(1))
      .subscribe(cityInfo => {
        this.city = cityInfo;
        this.meteoData = [];

        this.geoApi.getGridCoordinates(this.selRegion, this.selSubregion, this.selCity)
          .pipe(take(1))
          .subscribe(grid => {
            this.grid = grid;
            this.meteoData = [];

            this.meteoApi.getData(this.selRegion, this.selSubregion, this.selCity, 0, 0)
              .pipe(take(1))
              .subscribe(meteoApiData => this.processApiData(meteoApiData));
          });
      });
  }

  private processApiData(meteoApiData: MeteoData) {
    let mdEx: MeteoDailyDataEx[] = [];
    let md: MeteoDailyDataEx = undefined;

    if (meteoApiData && meteoApiData.data) {
      for(const date of Object.keys(meteoApiData.data)) {
        const data = meteoApiData.data[date];
        md = {
          date,
          forecast: data.forecast,
          tempFeel: data.tempFeel,
          tMaxActual: data.tMaxActual,
          tMaxNormal: data.tMaxNormal,
          tMinActual: data.tMinActual,
          tMinNormal: data.tMinNormal
        };
        mdEx.push(md);
      }

      let startDate = new Date(meteoApiData.calendarRange.start);
      let endDate = new Date(meteoApiData.calendarRange.end);

      while(startDate.getDay() != 1 /* Monday */) {
        startDate.setDate(startDate.getDate() - 1);
        md = {
          date: startDate.toISOString().slice(0, 10),
          forecast: undefined,
          tempFeel: undefined,
          tMaxActual: 0,
          tMaxNormal: 0,
          tMinActual: 0,
          tMinNormal: 0
        };
        mdEx.push(md);
      }

      while(endDate.getDay() != 0 /* Sunday */) {
        endDate.setDate(endDate.getDate() + 1);
        md = {
          date: endDate.toISOString().slice(0, 10),
          forecast: undefined,
          tempFeel: undefined,
          tMaxActual: 0,
          tMaxNormal: 0,
          tMinActual: 0,
          tMinNormal: 0
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
    }
  }

  public getInnerCellStyle(md: MeteoDailyDataEx) {
    let tempFeel = (md.tempFeel && md.tempFeel.length > 0) ? md.tempFeel : 'normal';
    return {
      'background': `${this.feelsLikeMap[tempFeel].color}`,
      'vertical-align': 'middle',
    }
  }

  public getOuterCellStyle(md: MeteoDailyDataEx) {
    const today = md.date === this.today;
    return {
      'border': `${today ? 5 : 4}px solid ${today ? 'teal' : 'transparent'}`,
      'height': '9vh',
      'vertical-align': 'middle',
    }
  }

  public getLabelStyle(md: MeteoDailyDataEx) {
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

  public getTooltip(md: MeteoDailyDataEx) {
    let tempFeel = (md.tempFeel && md.tempFeel.length > 0) ? md.tempFeel : 'normal';
    return this.feelsLikeMap[tempFeel].tip;
  }
}
