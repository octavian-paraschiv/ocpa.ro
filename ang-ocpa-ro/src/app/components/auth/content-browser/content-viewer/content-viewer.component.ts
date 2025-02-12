import { Component, Input, ViewChild } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';
import { WikiViewerComponent } from 'src/app/components/shared/wiki-viewer/wiki-viewer.component';
import { ContentUnit } from 'src/app/models/models-swagger';

@UntilDestroy()
@Component({
  selector: 'app-content-viewer',
  templateUrl: 'content-viewer.component.html'
})
export class ContentViewerComponent {
  @ViewChild('viewer', { static: true }) viewer: WikiViewerComponent;

  public display(node: ContentUnit) {
    let location = `${node.path}/${node.name}`;
    if (location.startsWith('wiki/'))
      location = location.replace('wiki/', '');
    this.viewer.displayLocation(location);
  }
}
