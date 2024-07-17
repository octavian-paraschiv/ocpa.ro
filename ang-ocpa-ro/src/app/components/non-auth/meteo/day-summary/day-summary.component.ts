import { Component, Input, OnChanges } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { MeteoDailyData, Unit } from 'src/app/models/meteo';
import { Helper } from 'src/app/services/helper';

@UntilDestroy()
@Component({
  selector: 'app-day-summary',
  templateUrl: './day-summary.component.html'
})
export class DaySummaryComponent implements OnChanges {
  @Input() data: MeteoDailyData = undefined; 
  @Input() unit = Unit.Metric; 

  isWeekend: boolean;
  isValid: boolean

  feelsLike: string;
  weatherType: string;
  desc: string;

  constructor(private helper: Helper) {
  }
  
  ngOnChanges() {
    this.isWeekend = new Date(this.data?.date).getDay() % 6 === 0;
    this.isValid = this.data?.forecast?.length > 0;
    let desc = '';
  
    if (this.isValid) {
      this.feelsLike = this.helper.feelsLikeTip(this?.data?.tempFeel);
      this.weatherType = this.helper.weatherType(this?.data?.forecast);
      
      if (this.weatherType?.length > 0) {
        if (desc.length > 0) {
          desc += '; ';
        }
        desc += this.weatherType;
      }

      if (this.feelsLike?.length > 0) {
        if (desc.length > 0) {
          desc += '; ';
        }
        desc += this.feelsLike;
      }
    }

    desc = desc.trim();
    if (desc.length > 0)
      this.desc = `<b>${desc}</b>`;
    else
      this.desc = '';
  }

  get labelClass(): string {
    return this.isWeekend ? 'day-summary-header-weekend' : 'day-summary-header-normal';
  }

  get isToday(): boolean {
    return this.helper.today === this.data?.date;
  }
}
