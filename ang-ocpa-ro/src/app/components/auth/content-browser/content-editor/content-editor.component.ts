import { Component } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';

@UntilDestroy()
@Component({
  selector: 'app-content-editor',
  templateUrl: 'content-editor.component.html'
})
export class ContentEditorComponent {
}
