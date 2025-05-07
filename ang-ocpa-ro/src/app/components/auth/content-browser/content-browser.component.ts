import { AfterViewInit, Component, ViewChild, inject } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { BaseComponent } from 'src/app/components/base/BaseComponent';
import { WikiViewerComponent } from 'src/app/components/shared/wiki-viewer/wiki-viewer.component';
import { Helper } from 'src/app/helpers/helper';
import { ContentUnit, ContentUnitType } from 'src/app/models/models-swagger';
import { ContentApiService } from 'src/app/services/api/content-api.service';
import { OverlayService } from 'src/app/services/overlay.service';

@UntilDestroy()
@Component({
  selector: 'app-content-browser',
  templateUrl: 'content-browser.component.html'
})
export class ContentBrowserComponent extends BaseComponent {
  oldContent = undefined;
  content = undefined;
  image = undefined;
  binary = undefined;

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
        this.overlay.show();

        this.contentService
          .getContent(this.contentPath)
          .pipe(untilDestroyed(this))
          .subscribe({
            next: res => {
              this.overlay.hide();
              this.editable = this.parseContent(res);
              if (this.editable)
                this.wikiViewer?.displayLocation(this.contentPath, false);
            },
            error: () => this.overlay.hide()
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
    this.overlay.show();
    const buffer = new TextEncoder().encode(this.content).buffer;
    this.contentService.uploadContent(this.contentPath, buffer, 'text/plain')
      .pipe(untilDestroyed(this))
      .subscribe({
        next: () => { 
          this.overlay.hide();
          this.wikiViewer?.displayLocation(this.contentPath, false); 
        },
        error: err => { 
          this.overlay.hide();
          this.popup.showError('failed to save: ' + err.toString());
          this.saveError = true;
        }
      });
  }
  
  parseContent(base64: string): boolean {
    let decodedBytes: string = undefined;
    
    this.content = undefined;
    this.image = undefined;
    this.binary = undefined;

    try {
      // Decode the Base64 string
      decodedBytes = window.atob(base64);
      if (decodedBytes?.length > 0 && decodedBytes.length < 1024 * 1024) {
        // Convert the decoded bytes to a string
        const decodedString = new TextDecoder().decode(new Uint8Array(decodedBytes.split('').map(char => char.charCodeAt(0))));
       
        // Check if the string contains valid UTF-8 characters
        if (/^[\x09-\x0D\x20-\x7F\x80-\u07FF]*$/.test(decodedString)) {
          this.content = decodedString;
          return true;
        }
      }
    } catch {
      // If decoding fails, it's likely binary data
      decodedBytes = base64;
    }

    this.content = '[Non-Readable Binary Data]';

    const header = Helper.getHeaderChars(decodedBytes);

    if (header.startsWith('FF D8 FF DB') ||
      header.startsWith('FF D8 FF E0') ||
      header.startsWith('FF D8 FF E1') ||
      header.startsWith('FF D8 FF EE') ||
      header.startsWith('FF 4F FF 51') ||
      header.startsWith('00 00 00 0C 6A 50 20 20 0D')) {
      this.image = `data:image/jpeg;base64,${base64}`;

    } else if (header.startsWith('47 49 46 38 37 61') ||
      header.startsWith('47 49 46 38 39 61')) {
      this.image = `data:image/gif;base64,${base64}`;

    } else if (header.startsWith('89 50 4E 47 0D 0A 1A 0A')) {
      this.image = `data:image/png;base64,${base64}`;

    } else if (header.startsWith('42 4D')) {
      this.image = `data:image/bmp;base64,${base64}`;

    } else {
      this.binary = decodedBytes;
    }

    return false;
  }  

  get previewStyle() {
      return { 
        'background-color': this.saveError ? 'coral' : 'white',
        'display': this.editable ? 'block' : 'none'
      };
  }
}
        