import { Component, HostListener, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Subject } from 'rxjs';
import { take, takeUntil, filter } from 'rxjs/operators';
import { City, GridCoordinates } from 'src/app/models/geography';
import { MeteoDailyData, MeteoData } from 'src/app/models/meteo';
import { GeographyApiService, MeteoApiService } from 'src/app/services/api-services';
import { Helper } from 'src/app/services/helper';

@UntilDestroy()
@Component({
  selector: 'app-meteo',
    templateUrl: './meteo.component.html'
})
export class MeteoComponent  implements OnInit {
  isFetching = false;
  fetchEvent$ = new Subject();

  regions: string[];
  selRegion: string;
  selSubregion: string;
  selCity: string;

  city: City;
  grid: GridCoordinates;

  meteoData: MeteoDailyData[] = [];
  todayData: MeteoDailyData;

  queryRegion: string = undefined;
  querySubregion: string = undefined;
  queryCity: string = undefined;
  queryUnit: string = undefined;

  selectedCity: City =  {};
  dropDownFocused = false;

  citiesBuffer: City[] = [];
  searchTerm = '';
  allCities: City[] = [];
  filteredCities: City[] = [];
  loadingCities = false;
  bufferSize = 15;

  selectedDate: string = undefined;
 
  constructor(private readonly geoApi: GeographyApiService,
    private readonly meteoApi: MeteoApiService,
    private readonly helper: Helper,
    private readonly route: ActivatedRoute) {
  }

  get dataGridStyle() {
    if (this.helper.today)
    return { 'height': `${this.dataGridHeight}px` };
  }

  get hint(): string {
    return 'To search your city, click/tap in the drop list below, then type the name or the city.';
  }

  get dataHint(): string {
    const location = `${this.lookupRegion} / ${this.lookupSubregion} / ${this.lookupCity}`;
    return (this.meteoData?.length > 0) ?
      `Forecast for: <b>${location}</b><br />Range: <b>${this.meteoData[0].date}...${this.meteoData[this.meteoData.length - 1].date}</b><br />Use the buttons below to go the desired date.` : 
      `Please wait while fetching data for: <b>${location}</b>`;
  }

  onDropDownFocused(focused: boolean) {
    this.dropDownFocused = focused;
  }

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
            }
          });
        }
      });
  }

  public onSmartCityChanged() {
    (document.activeElement as HTMLInputElement)?.blur();
    this.doFetch();
  }


  private doFetch() {
    this.meteoData = [];
    this.isFetching = true;
    this.fetchEvent$.next();

    this.meteoApi.getData(this.lookupRegion, this.lookupSubregion, this.lookupCity, 0, 0)
      .pipe(takeUntil(this.fetchEvent$), take(1), untilDestroyed(this))
      .subscribe(meteoApiData => {
        this.processApiData(meteoApiData);
        this.isFetching = false;
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
          let match = true;
          // All search terms must match
          for (let tt of terms) {
            let termMatch = false;
            termMatch ||= (city?.name ?? '').toLocaleUpperCase().includes(tt);
            termMatch ||= (city?.subregion ?? '').toLocaleUpperCase().startsWith(tt);
            termMatch ||= (city?.region ?? '').toLocaleUpperCase().startsWith(tt);
            match &&= termMatch;
            if (!match) break;
          }
          return match;
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
    }

    this.meteoData = mdEx;

    if (this.meteoData?.length > 0) {
      if (this.helper.today.localeCompare(this.meteoData[0].date) < 0)
        this.selectedDate = this.meteoData[0].date;
      else
        this.selectedDate = this.helper.today;
    }

    setTimeout(() => {
      const todayInfo = document.getElementById(`day_${this.selectedDate}`);
      todayInfo?.scrollIntoView();
      this.calculateDataGridHeight();
    }, 100);
  }

  meteoCellClass(date: string): string {
    return (
      (date === this.helper.today) ? 'meteo-td-today' : 
      (date === this.selectedDate) ? 'meteo-td-selected' : 
      'meteo-td'
    );
  }

  @HostListener('window:resize', ['$event'])
  onWindowResized(event) {
    this.calculateDataGridHeight();
  }

  dataGridHeight = 200;
  calculateDataGridHeight() {
    let height = window.innerHeight;
    height -= this.getAbsoluteHeight(document.getElementById('navbar'));
    height -= this.getAbsoluteHeight(document.getElementById('dHint'));
    height -= this.getAbsoluteHeight(document.getElementById('dControls'));
    height -= this.getAbsoluteHeight(document.getElementById('dSmartControls'));
    height -= this.getAbsoluteHeight(document.getElementById('dDataHint'));
    height -= this.getAbsoluteHeight(document.getElementById('btnDate'));
    height -= 10;
    this.dataGridHeight = height;
  }

  private getAbsoluteHeight(el: HTMLElement) {
    if (el) {
      var styles = window.getComputedStyle(el);
      var margin = parseFloat(styles['marginTop']) +
                   parseFloat(styles['marginBottom']);
    
      return Math.ceil(el.offsetHeight + margin);
    }
    return 0;
  }

  selectDate(daysDelta: number) {
    if (this.meteoData?.length > 0) {

      if (daysDelta === 0) {
        this.resetDate();

      } else {
        let newDate = this.helper.isoDate(this.helper.addDays(this.selectedDate, daysDelta));
        const startDate = this.meteoData[0].date;
        const endDate = this.meteoData[this.meteoData.length - 1].date;

        if (newDate.localeCompare(startDate) < 0)
          newDate = startDate;
        else if (newDate.localeCompare(endDate) > 0)
          newDate = endDate;

        this.selectedDate = newDate;
      }

      setTimeout(() => {
        const todayInfo = document.getElementById(`day_${this.selectedDate}`);
        todayInfo?.scrollIntoView();
        this.calculateDataGridHeight();
      }, 100);
    }
  }

  private resetDate() {
    if (this.meteoData?.length > 0) {
      if (this.helper.today.localeCompare(this.meteoData[0].date) < 0)
        this.selectedDate = this.meteoData[0].date;
      else
        this.selectedDate = this.helper.today;
    }
  }
}
