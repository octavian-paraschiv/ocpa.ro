import { Component, ViewChild, inject } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { BaseComponent } from 'src/app/components/base/BaseComponent';
import { WikiViewerComponent } from 'src/app/components/shared/wiki-viewer/wiki-viewer.component';
import { ContentUnit, ContentUnitType } from 'src/app/models/models-swagger';
import { ContentApiService } from 'src/app/services/api/content-api.service';

@UntilDestroy()
@Component({
  selector: 'app-content-browser',
  templateUrl: 'content-browser.component.html'
})
export class ContentBrowserComponent extends BaseComponent {
  oldContent = undefined;
  content = 'n/a';
  contentPath = '';
  keyDownTimeout = undefined;
  saveError = false;
  editable = true;

  @ViewChild('viewer', { static: true }) wikiViewer: WikiViewerComponent;
  
  private readonly contentService = inject(ContentApiService);
  
  filter = '*.md|*.png|*.bmp|*.jpg|*.jpeg|*.gif';

  onNodeSelected(node: ContentUnit) {
    if (node?.type === ContentUnitType.File &&
      node?.path?.length > 0 &&
      node?.name?.length > 0) {
        this.contentPath = `${node.path}/${node.name}`;
        this.contentService
          .getContent(this.contentPath)
          .pipe(untilDestroyed(this))
          .subscribe(res => {
            this.content = this.getText(res);
            this.wikiViewer?.displayLocation(this.contentPath, false);
        });
      }
  }

  onKeyDown() {
    if (this.keyDownTimeout)
      clearTimeout(this.keyDownTimeout);
    
    this.keyDownTimeout = setTimeout(() => {
      this.performSave();
      if (this.keyDownTimeout) {
        clearTimeout(this.keyDownTimeout);
        this.keyDownTimeout = undefined;
      }
    }, 5000);
  }

  performSave() {
    this.saveError = false;
    const buffer = new TextEncoder().encode(this.content).buffer;
    this.contentService.uploadContent(this.contentPath, buffer, 'text/plain')
      .pipe(untilDestroyed(this))
      .subscribe({
        next: () => { 
          this.wikiViewer?.displayLocation(this.contentPath, false); 
        },
        error: err => { 
          this.popup.showError('failed to save: ' + err.toString());
          this.saveError = true;
        }
      });
  }
  
  getText(base64: string): string {
    this.editable = false;
    try {
      // Decode the Base64 string
      const decodedBytes = window.atob(base64);

      if (decodedBytes?.length > 0 && decodedBytes.length < 1024 * 1024) {
        // Convert the decoded bytes to a string
        const decodedString = new TextDecoder().decode(new Uint8Array(decodedBytes.split('').map(char => char.charCodeAt(0))));
       
        // Check if the string contains valid UTF-8 characters
        if (/^[\x00-\x7F]*$/.test(decodedString)) {
          this.editable = true;
          return decodedString;
        }
      }
    } catch {
      // If decoding fails, it's likely binary data
    }
    return '[Non-Readable Binary Data]';
  }  

  get previewStyle() {
      return { 'background-color': this.saveError ? 'coral' : 'white' };
  }
}
        