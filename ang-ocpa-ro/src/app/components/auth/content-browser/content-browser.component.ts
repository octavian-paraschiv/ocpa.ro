import { Component, ViewChild } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';
import { ContentEditorComponent } from 'src/app/components/auth/content-browser/content-editor/content-editor.component';
import { ContentToolbarComponent } from 'src/app/components/auth/content-browser/content-toolbar/content-toolbar.component';
import { ContentViewerComponent } from 'src/app/components/auth/content-browser/content-viewer/content-viewer.component';
import { ContentTreeComponent } from 'src/app/components/shared/content-tree/content-tree.component';
import { ContentUnit } from 'src/app/models/models-swagger';

@UntilDestroy()
@Component({
  selector: 'app-content-browser',
  templateUrl: 'content-browser.component.html'
})
export class ContentBrowserComponent {
  @ViewChild('tree', { static: true }) tree: ContentTreeComponent;
  @ViewChild('toolbar', { static: true }) toolbar: ContentToolbarComponent;
  @ViewChild('editor', { static: true }) editor: ContentEditorComponent;
  @ViewChild('viewer', { static: true }) viewer: ContentViewerComponent;
  
    filter = '*.md|*.png|*.bmp|*.jpg|*.jpeg|*.gif';

  onNodeSelected(node: ContentUnit) {
    this.viewer.display(node);
  }
}
