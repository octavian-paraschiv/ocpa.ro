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
    templateUrl: './meteo.component.html'
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

  selectedCity: City =  {};
  
  constructor(private readonly geoApi: GeographyApiService,
    private readonly meteoApi: MeteoApiService,
    private readonly route: ActivatedRoute) {
  }

  get hint(): string {
    return (this.geoApi?.cities?.length > 0) ?
      'Please click/tap in the drop list below. If you can already see your city, then tap or click it.<br>' +
      'Otherwise, please type the city name (or the name of the country or district where your city is located).<br>' +
      'The drop list will narrow to the cities that match the typed text. If you see your city, tap or click on it.' : 
      'Please select the desired city for the forecast.&nbsp;You may need to select the Region and Country/District first.';
  }

  citiesBuffer: City[] = [];
  
  searchTerm = '';
  allCities: City[] = [];

  filteredCities: City[] = [];

  loadingCities = false;
  bufferSize = 15;

  onScrollToEnd() {
    this.fetchMore();
  }

  onScroll(event: { end: number }) {
    if (this.loadingCities || this.filteredCities.length <= this.citiesBuffer.length) {
        return;
    }

    if ((event.end ?? 0) + 5 >= this.citiesBuffer.length) {
        this.fetchMore();
    }
  }

  onSearchCleared() {
    this.initialize();
  }

  onSearch(event: {term: string}) {
    if (event?.term?.length > 0 && this.searchTerm !== event?.term) {
      this.searchTerm = event?.term;
      this.loadingCities = true;
      this.filterCities();
    }

    setTimeout(() => {
      this.loadingCities = false;
      const scrollContainer = document.querySelector('.ng-dropdown-panel-items');
      if (scrollContainer) {
        scrollContainer.scrollTop = 0;
      }
    }, 100);
  }

  searchCities(): boolean {
    return true;
  }
 
  private fetchMore() {
      const len = this.citiesBuffer.length;
      const more = (this.filteredCities).slice(len, this.bufferSize + len);
      this.citiesBuffer = this.citiesBuffer.concat(more);
  }
  
  ngOnInit(): void {
    this.initialize();
  }

  private initialize() {
    this.searchTerm = '';
    this.allCities = (this.geoApi?.cities);
    this.filterCities();

    this.meteoData = [];
    this.selectedCity = {};
    this.today = new Date().toISOString().slice(0, 10);

    this.route.queryParams
      .pipe(take(1), untilDestroyed(this))
      .subscribe(params => {

        this.queryRegion = params['region'];
        this.querySubregion = params['subregion'];
        this.queryCity = params['city'];
        this.queryUnit = params['unit'];

        if (this.allCities?.length > 0) {
          const defCity = this.allCities.find(c => c.name.indexOf('Bucuresti') > 0 && c.region === 'Romania');
          this.selectedCity.name = this.queryCity ?? defCity.name;
          this.selectedCity.region = this.queryRegion ?? defCity.region;
          this.selectedCity.subregion = this.querySubregion ?? defCity.subregion;
          this.onSmartCityChanged();

        } else {
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
        }
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

  public onSmartCityChanged() {
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

    this.geoApi.getCityInfo(this.lookupRegion, this.lookupSubregion, this.lookupCity)
      .pipe(takeUntil(this.fetchEvent$), take(1), untilDestroyed(this))
      .subscribe(cityInfo => {
        this.city = cityInfo;
        this.meteoData = [];
        this.geoApi.getGridCoordinates(this.lookupRegion, this.lookupSubregion, this.lookupCity)
          .pipe(takeUntil(this.fetchEvent$), take(1), untilDestroyed(this))
          .subscribe(grid => {
            this.grid = grid;
            this.meteoData = [];
            this.meteoApi.getData(this.lookupRegion, this.lookupSubregion, this.lookupCity, 0, 0)
              .pipe(takeUntil(this.fetchEvent$), take(1), untilDestroyed(this))
              .subscribe(meteoApiData => {
                this.processApiData(meteoApiData);
                this.isFetching = false;
              });
          });
      });
  }

  private get lookupRegion(): string {
    return this.selectedCity?.region ?? this.selRegion;
  }
  private get lookupSubregion(): string {
    return this.selectedCity?.subregion ?? this.selSubregion;
  }
  private get lookupCity(): string {
    return this.selectedCity?.name ?? this.selCity;
  }

  private filterCities() {
    const allCities = this.allCities ?? [];
    if (allCities.length > 5) {
      if (this.searchTerm?.length > 0) {
        const terms = this.searchTerm.toLocaleUpperCase().split(' ').filter(t => t?.length > 0);
        this.filteredCities = GeographyApiService.SortCities(terms, allCities.filter(city => {
          for (let tt of terms) {
            if ((city?.name ?? '').toLocaleUpperCase().includes(tt))
              return true;
            if ((city?.subregion ?? '').toLocaleUpperCase().includes(tt))
              return true;
            if ((city?.region ?? '').toLocaleUpperCase().includes(tt))
              return true;
          }
          return false;
        }));
  
      } else {
        // no filter specified => no filtering needed
        this.filteredCities = allCities;
      }

    } else {
      // Less than 5 cities to show => no filtering needed
      this.filteredCities = allCities;
    }

    this.citiesBuffer = (this.filteredCities.length > 5) ?
      this.filteredCities.slice(0, 5) : this.filteredCities;
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

    }
  }
}
