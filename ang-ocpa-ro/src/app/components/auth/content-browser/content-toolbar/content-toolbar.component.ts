import { Component } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';

@UntilDestroy()
@Component({
  selector: 'app-content-toolbar',
  templateUrl: 'content-toolbar.component.html'
})
export class ContentToolbarComponent {
}
