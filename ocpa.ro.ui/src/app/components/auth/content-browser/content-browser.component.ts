import { AfterViewInit, Component, ViewChild, inject } from '@angular/core';
import { faFileEdit, faFileText, faFolder, faTrash, faUpload } from '@fortawesome/free-solid-svg-icons';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Observable, of } from 'rxjs';
import { first, switchMap } from 'rxjs/operators';
import { NodeNameDialogComponent } from 'src/app/components/auth/content-browser/node-name-dialog/node-name-dialog.component';
import { BaseComponent } from 'src/app/components/base/BaseComponent';
import { ContentTreeComponent } from 'src/app/components/shared/content-tree/content-tree.component';
import { MessageBoxComponent } from 'src/app/components/shared/message-box/message-box.component';
import { WikiViewerComponent } from 'src/app/components/shared/wiki-viewer/wiki-viewer.component';
import { Helper } from 'src/app/helpers/helper';
import { MessageBoxOptions } from 'src/app/models/models-local';
import { ContentUnit, ContentUnitType } from 'src/app/models/models-swagger';
import { ContentApiService } from 'src/app/services/api/content-api.service';

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
  currentNode: ContentUnit = undefined;

  keyDownTimeout = undefined;
  saveError = false;
  editable = true;

  folder = faFolder;
  file = faFileText;
  upload = faUpload;
  delete = faTrash;
  rename = faFileEdit;
  size = "grow-2";

  @ViewChild('viewer', { static: true }) wikiViewer: WikiViewerComponent;
  @ViewChild('tree', { static: true }) tree: ContentTreeComponent;

  private readonly contentService = inject(ContentApiService);

  onNodeSelected(node: ContentUnit) {
    this.currentNode = node;
    if (node?.path?.length > 0 && node?.name?.length > 0) {
      this.contentPath = `${node.path}/${node.name}`;
      if (node?.type === ContentUnitType.File) {
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
      } else {
        this.content = undefined;
        this.image = undefined;
        this.binary = undefined;
        this.wikiViewer?.reset();
      }
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
    this.content = undefined;
    this.image = undefined;
    this.binary = undefined;

    const decodedText = Helper.tryDecodeAsText(base64);
    if (decodedText?.length > 0) {
      this.content = decodedText;
      return true;
    }

    this.content = '[Non-Readable Binary Data]';

    const decodedImage = Helper.tryDecodeAsImage(base64);
    if (decodedImage?.length > 0) {
      this.image = decodedImage;
      return false;
    }

    this.binary = base64;
    return false;
  }

  get previewStyle() {
    return {
      'background-color': this.saveError ? 'coral' : 'white',
      'display': this.editable ? 'block' : 'none'
    };
  }

  isActionAllowed(action: string) {
    switch (action) {
      case 'folder':
      case 'file':
      case 'upload':
        return this.currentNode?.type === ContentUnitType.Folder || this.currentNode?.type === ContentUnitType.MarkdownIndexFolder;

      case 'rename':
        return this.currentNode?.path?.length > 0;

      case 'delete':
        return this.currentNode && !(this.currentNode?.children?.length > 0);
    }
    return false;
  }

  getTreeButtonClass(action: string) {
    const enabled = this.isActionAllowed(action);
    return `content-browser-button-${enabled ? action : 'disabled'}`;
  }

  onNewFolder() {
    if (!this.isActionAllowed('folder'))
      return;

    NodeNameDialogComponent.showDialog(this.dialog, {
      parent: this.currentNode,
      node: { type: ContentUnitType.Folder }
    }).pipe(
      first(),
      untilDestroyed(this),
      switchMap(path => path?.length > 0 ? this.contentService.createFolder(path) : of(undefined as ContentUnit))
    ).subscribe({
      next: cu => {
        if (cu?.path?.length > 0) {
          this.tree.reloadAndSelect(`${cu.path}/${cu.name}`);
          this.popup.showSuccess('content-browser.new-folder-success', { name: cu.name });
        }
      },
      error: err => {
        this.popup.showSuccess('content-browser.new-folder-error', { name: this.currentNode.name });
      }
    });
  }

  onNewFile() {
    if (!this.isActionAllowed('file'))
      return;

    NodeNameDialogComponent.showDialog(this.dialog, {
      parent: this.currentNode,
      node: { type: ContentUnitType.File }
    }).pipe(
      first(),
      untilDestroyed(this),
      switchMap(path => path?.length > 0 ? this._createNewFile(path) : of(undefined as ContentUnit))
    ).subscribe({
      next: cu => {
        if (cu?.path?.length > 0) {
          this.tree.reloadAndSelect(`${cu.path}/${cu.name}`);
          this.popup.showSuccess('content-browser.new-file-success', { name: cu.name });
        }
      },
      error: err => {
        this.popup.showSuccess('content-browser.new-file-error', { name: this.currentNode.name });
      }
    });
  }

  onRename() {
    if (!this.isActionAllowed('rename'))
      return;

    NodeNameDialogComponent.showDialog(this.dialog, {
      node: this.currentNode
    }).pipe(
      first(),
      untilDestroyed(this),
      switchMap(path => path?.length > 0 ? this._rename(path) : of(undefined as ContentUnit))
    ).subscribe({
      next: cu => {
        if (cu?.name?.length > 0) {
          if (!(cu.path?.length > 0))
            cu.path = '.';
          this.tree.reloadAndSelect(`${cu.path}/${cu.name}`);
          this.popup.showSuccess('content-browser.rename-success', { name: cu.name });
        }
      },
      error: err => {
        this.popup.showError('content-browser.rename-error', { name: this.currentNode?.name });
      }
    });
  }

  _rename(newPath: string): Observable<ContentUnit> {
    const oldPath = `${this.currentNode.path}/${this.currentNode.name}`;
    return this.contentService.moveContent(oldPath, newPath);
  }

  _createNewFile(path: string): Observable<ContentUnit> {
    const buffer = new TextEncoder().encode(path).buffer;
    return this.contentService.uploadContent(path, buffer, 'text/plain');
  }

  onUploadFile() {
    if (!this.isActionAllowed('upload'))
      return;

    this.overlay.show();
    this.fileOpen(fileData => {
      this.contentService.uploadContent(fileData.path, fileData.content, fileData.type)
        .pipe(untilDestroyed(this))
        .subscribe({
          next: () => {
            this.tree.reloadAndSelect(this.currentNode.path);
            this.popup.showSuccess('content-browser.upload-success', { name: this.currentNode?.name });
          },
          error: err => {
            this.popup.showError('content-browser.upload-error', { name: this.currentNode?.name });
          }
        });
    });
  }

  onDelete() {
    if (!this.isActionAllowed('delete'))
      return;

    MessageBoxComponent.show(this.dialog, {
      title: this.translate.instant('title.confirm'),
      message: this.translate.instant('content-browser.delete-node', { name: this.currentNode?.name })
    } as MessageBoxOptions)
      .pipe(untilDestroyed(this))
      .subscribe(res => {
        if (res) {
          this.overlay.show();
          this.contentService.deleteContent(this.contentPath)
            .pipe(untilDestroyed(this))
            .subscribe({
              next: () => {
                this.tree.reloadAndSelect(this.currentNode.path);
                this.popup.showSuccess('content-browser.delete-success', { name: this.currentNode?.name });
              },
              error: err => {
                this.popup.showError('content-browser.delete-error', { name: this.currentNode?.name });
              }
            });
        }
      });
  }


  private fileOpen(callback: (fileData: { path: string, content: ArrayBuffer, type: string }) => void) {
    const input = document.createElement('input');
    input.type = 'file';
    input.onchange = () => {
      const file = input.files[0];
      if (file) {
        file.arrayBuffer().then((content) => {
          callback({ 
            path: `${this.currentNode.path}/${this.currentNode.name}/${file.name}`, 
            content, 
            type: file.type });
        });
      }
    };
    input.onabort = () => this.overlay.hide();
    input.oncancel = () => this.overlay.hide();
    input.onclose = () => this.overlay.hide();
    input.click();
  }

}
