import { formatDate } from "@angular/common";
import { Component, Input, OnChanges, OnDestroy, OnInit } from "@angular/core";
import { UntilDestroy } from "@ngneat/until-destroy";
import { MeteoDailyData, MeteoData } from "src/app/models/meteo";

@UntilDestroy()
@Component({
  selector: 'app-calendar',
  templateUrl: './calendar.component.html',
  styleUrls: [ './calendar.component.scss' ]
})
export class CalendarComponent
implements OnInit, OnDestroy, OnChanges {

  @Input() data: MeteoData = null;

  startDate: Date = new Date();
  endDate: Date = new Date();
  dates: Date[] = [];

  ngOnInit() {
    this.setDates();
  }

  ngOnDestroy() {
  }

  ngOnChanges() {
    this.setDates();
  }

  public setDates() {
    try {

      if (Date.parse(this.data?.CalendarRange?.Start) > 0) {
        this.startDate = new Date(this.data?.CalendarRange?.Start);
      }

      if (Date.parse(this.data?.CalendarRange?.End) > 0) {
        this.endDate = new Date(this.data?.CalendarRange?.End);
      }

      if (this.endDate.getDate() < this.startDate.getDate()) {
        this.endDate.setDate(this.startDate.getDate());
      }

      while(this.startDate.getDay() !== 1) {
        this.startDate.setDate(this.startDate.getDate() - 1);
      }

      while(this.endDate.getDay() !== 0) {
        this.endDate.setDate(this.endDate.getDate() + 1);
      }

      this.dates = [];
      for(let d = this.startDate; d <= this.endDate; d.setDate(d.getDate() + 1)) {
        this.dates.push(new Date(d));
      }
    } catch {
    }
  }

  public isWeekEnd(d: Date): boolean {
    return (d.getDay() === 0);
  }

  public getData(date: string): MeteoDailyData {
    try {
      const ds = formatDate(date, 'yyyy-MM-dd', 'en-US')
      return this.data.Data[ds];
    } catch {
      const x: MeteoDailyData = {
        Forecast: null,
        TempFeel: '',
        TMaxActual: '',
        TMaxNormal: '',
        TMinActual: '',
        TMinNormal: '',
      };
    }
  }
}
