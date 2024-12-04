import { OnInit, Component, ElementRef, HostListener, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Subject } from 'rxjs';
import { take, takeUntil } from 'rxjs/operators';
import { CityDetail, GridCoordinates, MeteoDailyData, MeteoData } from 'src/app/models/models-swagger';
import { GeographyApiService, MeteoApiService } from 'src/app/services/api-services';
import { Helper } from 'src/app/services/helper';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { Chart, ChartConfiguration, ChartOptions, TooltipItem } from "chart.js";
import { Unit } from 'src/app/models/models-local';
import { DistancePipe, TempPipe, VolumePipe } from 'src/app/services/unit-transform-pipe';
import annotationPlugin, { AnnotationOptions } from 'chartjs-plugin-annotation';
import { BaseChartDirective } from 'ng2-charts';
import { TranslateService } from '@ngx-translate/core';

Chart.register(annotationPlugin);

@UntilDestroy()
@Component({
  selector: 'app-meteo-data-browser',
  templateUrl: './meteo-data-browser.component.html'
})
export class MeteoDataBrowserComponent implements OnInit  {
  @ViewChild(BaseChartDirective) chart: BaseChartDirective | undefined;

  icons = fas;
  plugins = [annotationPlugin];

  dbi = -1;
  defaultHint = true;

  // CONVERT UNITS
  unit = Unit.Metric;

  hint = '';

  initialized = false;
  isFetching = false;
  fetchEvent$ = new Subject();

  regions: string[];
  selRegion: string;
  selSubregion: string;
  selCity: string;

  city: CityDetail;
  grid: GridCoordinates;

  meteoData: MeteoDailyData[] = [];
  selMeteoData: MeteoDailyData[] = [];

  queryRegion: string = undefined;
  querySubregion: string = undefined;
  queryCity: string = undefined;
  queryUnit: string = undefined;

  selectedCity: CityDetail =  {};
  dropDownFocused = false;

  citiesBuffer: CityDetail[] = [];
  searchTerm = '';
  allCities: CityDetail[] = [];
  filteredCities: CityDetail[] = [];
  loadingCities = false;
  bufferSize = 15;

  selectedDate: string = undefined;

  displayColumns = [ 'date', 'symbol', 'summary', 'temp', 'precip', 'risks' ];
 
  lineChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: [],
  };

  lineChartOptions: ChartOptions<'line'> = {
    responsive: true,
    animation: false,
    plugins: {
      tooltip: {
        callbacks: {
          label: this.buildTooltip
        }
      },
      annotation: {
        annotations: {
          line1: {
            type: 'line',
            xMin: Helper.today,
            xMax: Helper.today,
            display: true,
            borderColor: 'rgb(0, 179, 179)',
            borderWidth: 2,
            label: {
              content: `${Helper.today}`,
              backgroundColor: 'rgb(0, 179, 179)',
              display: true,
              position: 'end'
            }
          },
          line2: {
            type: 'line',
            xMin: 0,
            xMax: 0,
            display: false,
            borderColor: 'rgb(252, 172, 163)',
            borderWidth: 2,
            label: {
              display: true,
              position: 'end',
              backgroundColor: 'rgb(252, 172, 163)'
            }
          }
        }
      }
    },
  };

  

  constructor(
    private readonly translate: TranslateService,
    private readonly geoApi: GeographyApiService,
    private readonly meteoApi: MeteoApiService,
    private readonly route: ActivatedRoute) {
  }

  ngOnInit() {
    this.hint = this.translate.instant('meteo.default-hint');
  }

  get dataGridStyle() {
    if (Helper.today)
      return { 'height': `${this.dataGridHeight}px` };

    return undefined;
  }

  get dataHint(): string {
    const key = this.isFetching ? 'meteo.data-hint-fetching' :
      (this.meteoData?.length > 0) ? 'meteo.data-hint' : '';

    return this.translate.instant(key, { 
      name: location, 
      startDate: this.meteoData[0].date,
      endDate: this.meteoData[this.meteoData.length - 1].date
    });
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
    this.init();
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
  
  public initWithParams(dbi: number, defaultHint: boolean) {
    this.dbi = dbi;
    this.defaultHint = defaultHint;
    this.init();
  }

  public init() {
    this.initialized = true;
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
          const defCity = this.allCities.find(c => c.name.indexOf('Bucuresti') > 0 && c.regionName === 'Romania');
          this.selectedCity.name = this.queryCity ?? defCity.name;
          this.selectedCity.regionName = this.queryRegion ?? defCity.regionName;
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

    this.meteoApi.getData(this.dbi, this.lookupRegion, this.lookupSubregion, this.lookupCity, 0, 0)
      .pipe(takeUntil(this.fetchEvent$), take(1), untilDestroyed(this))
      .subscribe(meteoApiData => {
          this.hint = this.defaultHint ?
            this.translate.instant('meteo.default-hint') : 
            this.translate.instant('meteo.alt-hint', 
              { name: meteoApiData?.name, length: meteoApiData?.dataCount ?? 0 });

        this.processApiData(meteoApiData);
        this.isFetching = false;
      });
  }

  private get lookupRegion(): string {
    return this.selectedCity?.regionName ?? this.selRegion;
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
            termMatch ||= (city?.regionName ?? '').toLocaleUpperCase().startsWith(tt);
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
    this.meteoData = [];
    this.selMeteoData = [];

    this.lineChartData = {
      labels: [],
      datasets: []
    };

    if (meteoApiData?.data) {
      for(const date of Object.keys(meteoApiData.data)) {
        const data = meteoApiData.data[date];
        data.date = date;
        this.meteoData.push(data);
      }
    }

    if (this.meteoData?.length > 0) {
      this.refreshGrid();
      if (Helper.today.localeCompare(this.meteoData[0].date) < 0)
        this.selectedDate = this.meteoData[0].date;
      else
        this.selectedDate = Helper.today;
    }

    this.refreshSelMeteoData();
  }

  private refreshGrid() {
    if (!Helper.isMobile()) {
      this.lineChartData = {
        labels: this.meteoData.map(md => md.date),
        datasets: [
          {
            data: this.meteoData.map(md => md.tMaxActual ?? 0),
            label: this.translate.instant('meteo.t-max'),
            tension: 0.5,
            borderColor: 'red',
            borderWidth: 2,
            pointRadius: 0,
            pointHitRadius: 5,
          },
          {
            data: this.meteoData.map(md => md.tMinActual ?? 0),
            label: this.translate.instant('meteo.t-min'),
            tension: 0.5,
            borderColor: 'blue',
            borderWidth: 2,
            pointRadius: 0,
            pointHitRadius: 5,
          }
        ]};

      if (this.meteoData.filter(md => this.inst(md) > 0)?.length > 0) {
        this.lineChartData.datasets = [ ...this.lineChartData.datasets,
          {
            data: this.meteoData.map(md => this.inst(md)),
            label: this.translate.instant('meteo.t-storm'),
            tension: 0.5,
            borderColor: 'lime',
            backgroundColor: 'lime',
            fill: true,
            borderWidth: 1,
            pointRadius: 0,
            pointHitRadius: 5,
          }
        ];
      }              

      if (this.meteoData.filter(md => md.snow > 0)?.length > 0) {
        this.lineChartData.datasets = [ ...this.lineChartData.datasets,
          {
            data: this.meteoData.map(md => md.snow ?? 0),
            label: this.translate.instant('meteo.snow-new'),
            tension: 0.5,
            borderColor: 'lightblue',
            backgroundColor: 'lightblue',
            fill: true,
            borderWidth: 1,
            pointRadius: 0,
            pointHitRadius: 5,
          }
        ];
    }

      if (this.meteoData.filter(md => md.snowCover > 0)?.length > 0) {
        this.lineChartData.datasets = [ ...this.lineChartData.datasets,
          {
            data: this.meteoData.map(md => md.snowCover ?? 0),
            label: this.translate.instant('meteo.snow-total'),
            tension: 0.5,
            borderColor: 'cadetblue',
            backgroundColor: 'cadetblue',
            fill: true,
            borderWidth: 1,
            pointRadius: 0,
            pointHitRadius: 5,
          }
        ];
      }

      if (this.meteoData.filter(md => md.rain > 0)?.length > 0) {
        this.lineChartData.datasets = [ ...this.lineChartData.datasets,
          {
            data: this.meteoData.map(md => md.rain ?? 0),
            label: this.translate.instant('meteo.rain'),
            tension: 0.5,
            borderColor: 'lightgray',
            backgroundColor: 'lightgray',
            fill: true,
            borderWidth: 1,
            pointRadius: 0,
            pointHitRadius: 5,
          }
        ];
      }
    }
  }

  private refreshSelMeteoData() {

    setTimeout(() => {
      const height = this.calculateDataGridHeight();

      let delta = 3;

      const chartInstance = this.chart?.chart;

      if (Helper.isMobile())
        delta = Math.max(Math.floor(height / 120) - 1, 3);

      else if (chartInstance) {
        const line2 = chartInstance.options.plugins.annotation.annotations['line2'] as AnnotationOptions<'line'>;
        if (line2) {
          line2.xMin = this.selectedDate ?? '';
          line2.xMax = this.selectedDate ?? '';
          line2.display = (this.selectedDate && this.selectedDate !== Helper.today);
          line2.label.content = this.selectedDate;
          chartInstance.update();
        }
      }
      
      let start = Helper.isoDate(Helper.addDays(this.selectedDate, -delta));
      let end = Helper.isoDate(Helper.addDays(this.selectedDate, delta));

      if (start.localeCompare(this.meteoData[0].date) < 0)
        start = this.meteoData[0].date;

      if (end.localeCompare(this.meteoData[this.meteoData.length - 1].date) > 0)
        end = this.meteoData[this.meteoData.length - 1].date;

      this.selMeteoData = this.meteoData.filter(md => 
        md.date.localeCompare(start) >= 0 && md.date.localeCompare(end) <= 0);
    }, 100);
  }

  meteoCellClass(date: string): string {
    return (
      (date === Helper.today) ? 'meteo-td-today' : 
      (date === this.selectedDate) ? 'meteo-td-selected' : 
      'meteo-td'
    );
  }

  @HostListener('window:resize', ['$event'])
  onWindowResized(_event: any) {
    this.refreshSelMeteoData();
  }

  dataGridHeight = 0;
  calculateDataGridHeight(): number{
    let height = window.innerHeight;

    if (Helper.isMobile())
      height -= this.getAbsoluteHeight(document.getElementById('navbar'));

    height -= this.getAbsoluteHeight(document.getElementById('dHint'));
    height -= this.getAbsoluteHeight(document.getElementById('dControls'));
    height -= this.getAbsoluteHeight(document.getElementById('dSmartControls'));

    height -= this.getAbsoluteHeight(document.getElementById('dDataHint'));
    height -= this.getAbsoluteHeight(document.getElementById('btnDate'));
    height -= 15;

    this.dataGridHeight = height;
    return height;
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
        let newDate = Helper.isoDate(Helper.addDays(this.selectedDate, daysDelta));
        const startDate = this.meteoData[0].date;
        const endDate = this.meteoData[this.meteoData.length - 1].date;

        if (newDate.localeCompare(startDate) < 0)
          newDate = startDate;
        else if (newDate.localeCompare(endDate) > 0)
          newDate = endDate;

        this.selectedDate = newDate;
      }

      this.refreshSelMeteoData();
    }
  }

  private resetDate() {
    if (this.meteoData?.length > 0) {
      if (Helper.today.localeCompare(this.meteoData[0].date) < 0)
        this.selectedDate = this.meteoData[0].date;
      else
        this.selectedDate = Helper.today;
    }
  }


  dateCellClass(md: MeteoDailyData): string {
    const isWeekend = new Date(md?.date).getDay() % 6 === 0;
    return isWeekend ? 'date-cell-weekend' : 'date-cell';
  }

  leftCellClass(md: MeteoDailyData): string {
    if (Helper.today.localeCompare(md?.date) === 0)
      return 'left-cell-today';
    if (this.selectedDate.localeCompare(md?.date) === 0)
      return 'left-cell-selected';
    return '';
  }
  centerCellClass(md: MeteoDailyData): string {
    if (Helper.today.localeCompare(md?.date) === 0)
      return 'center-cell-today';
    if (this.selectedDate.localeCompare(md?.date) === 0)
      return 'center-cell-selected';
    return '';
  }
  rightCellClass(md: MeteoDailyData): string {
    if (Helper.today.localeCompare(md?.date) === 0)
      return 'right-cell-today';
    if (this.selectedDate.localeCompare(md?.date) === 0)
      return 'right-cell-selected';
    return '';
  }

  summary(md: MeteoDailyData): string {
    let desc = '';
    const isValid = md?.forecast?.length > 0;
  
    if (isValid) {
      const feelsLike = this.translate.instant(`meteo.feels-like.${md?.tempFeel ?? 'normal'}`);
      const weatherType = this.translate.instant(`meteo.weather-type.${md?.forecast ?? '00'}`);
      
      if (weatherType?.length > 0) {
        if (desc.length > 0) {
          desc += '; ';
        }
        desc += weatherType;
      }

      if (feelsLike?.length > 0) {
        if (desc.length > 0) {
          desc += '<br>';
        }
        desc += feelsLike;
      }
    }

    desc = desc.trim();
    if (desc.length > 0)
      desc = `${desc}`;
    else
      desc = '';

    return desc;
  }

  inst(md: MeteoDailyData): number {
    let val = Math.max(0, 6 - md?.instability ?? 0);
    return val;
  }

  public chartClicked(e: any): void {
    if (e.active.length > 0) {
      const idx = e.active[0].index;
      this.selectedDate = this.meteoData[idx].date;
      this.refreshSelMeteoData();
    }
  }

  public buildTooltip(item: TooltipItem<"line">): string {
    switch(item?.dataset?.label) {
      case this.translate.instant('meteo.t-min'):
      case this.translate.instant('meteo.t-max'):
        return `${item?.dataset?.label}: ${TempPipe._transform(item?.raw as number, this.unit)}`;;

      case this.translate.instant('meteo.rain'):
        return `${item?.dataset?.label}: ${VolumePipe._transform(item?.raw as number, this.unit)}`;;

      case this.translate.instant('meteo.snow-new'):
      case this.translate.instant('meteo.snow-total'):
        return `${item?.dataset?.label}: ${DistancePipe._transform(item?.raw as number, this.unit)}`;
    }

    return `${item?.dataset?.label}: ${item?.raw}`;
  }
}
