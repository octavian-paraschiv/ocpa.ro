import { Component, Input, OnChanges } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { MeteoDailyData, Unit } from 'src/app/models/meteo';
import { Helper } from 'src/app/services/helper';

@UntilDestroy()
@Component({
  selector: 'app-day-summary',
  templateUrl: './day-summary.component.html',
  styleUrls: ['../../../../assets/styles/site.css']
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
  
    if (this.isValid) {
      this.feelsLike = this.helper.feelsLikeTip(this?.data?.tempFeel);
      this.weatherType = this.helper.weatherType(this?.data?.forecast);
      
      this.desc = '';
      if (this.feelsLike?.length > 0) {
        this.desc += this.feelsLike;
      }
      if (this.weatherType?.length > 0) {
        if (this.desc.length > 0) {
          this.desc += '<br>';
        }
        this.desc += this.weatherType;
      }
    }
  }
}