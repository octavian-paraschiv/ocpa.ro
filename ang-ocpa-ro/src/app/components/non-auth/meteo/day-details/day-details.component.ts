import { Component, Input, OnChanges } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { MeteoDailyData, Unit } from 'src/app/models/meteo';
import { Helper } from 'src/app/services/helper';

@UntilDestroy()
@Component({
  selector: 'app-day-details',
  templateUrl: './day-details.component.html'
})
export class DayDetailsComponent {
  @Input() data: MeteoDailyData = undefined; 
  @Input() unit = Unit.Metric; 
}
