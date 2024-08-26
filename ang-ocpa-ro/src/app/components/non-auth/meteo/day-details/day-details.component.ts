import { Component, Input } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';
import { Unit } from 'src/app/models/models-local';
import { MeteoDailyData } from 'src/app/models/models-swagger';

@UntilDestroy()
@Component({
  selector: 'app-day-details',
  templateUrl: './day-details.component.html'
})
export class DayDetailsComponent {
  @Input() data: MeteoDailyData = undefined; 
  @Input() unit = Unit.Metric; 
}
