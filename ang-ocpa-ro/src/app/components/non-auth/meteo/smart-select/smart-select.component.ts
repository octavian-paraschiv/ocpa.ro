import { Component, Input, Output, EventEmitter } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';


@UntilDestroy()
@Component({
  selector: 'app-smart-select',
  templateUrl: './smart-select.component.html'
})
export class SmartSelectComponent {
  @Input() items: string[] = [];
  @Input() selValue = '';
  @Output() selValueChanged = new EventEmitter<string>();

}
