import { Component, Input, OnChanges } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';


@UntilDestroy()
@Component({
  selector: 'app-day-risks',
  templateUrl: './day-risks.component.html',
  styleUrls: ['../../../../assets/styles/site.css']
})
export class DayRisksComponent {
  @Input() risks: string[] = undefined;
}
