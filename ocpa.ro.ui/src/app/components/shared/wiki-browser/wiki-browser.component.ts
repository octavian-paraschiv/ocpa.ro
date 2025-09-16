import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { BaseComponent } from 'src/app/components/base/BaseComponent';
import { ContentTreeComponent } from 'src/app/components/shared/content-tree/content-tree.component';
import { WikiViewerComponent } from 'src/app/components/shared/wiki-viewer/wiki-viewer.component';
import { ContentUnit, ContentUnitType } from 'src/app/models/models-swagger';

@UntilDestroy()
@Component({
    selector: 'app-wiki-browser',
    templateUrl: './wiki-browser.component.html'
})
export class WikiBrowserComponent extends BaseComponent implements OnInit {
  contentPath = '';
  currentNode: ContentUnit = undefined;
  
  @ViewChild('viewer', { static: true }) wikiViewer: WikiViewerComponent;
  @ViewChild('tree', { static: true }) wikiTree: ContentTreeComponent;

  private readonly route = inject(ActivatedRoute);

  ngOnInit(): void {
    this.route.params
      .pipe(untilDestroyed(this))
      .subscribe(() => this.reloadLocation());
  }

  reloadLocation() {
    try { 
      const url = this.route?.snapshot?.url;
      const location = url.map(s => s.path).join('/').replace('wiki-browser/', 'wiki/');
      setTimeout(() => {
        if (this.wikiTree && location?.length > 0) {
          this.wikiTree.path = location;
          this.wikiTree.reloadAndSelect(undefined);
        }
      }, 500);
    }                
    catch { }
  }

  onNodeSelected(node: ContentUnit) {
    let rendered = false;
    this.currentNode = node;
    if (node?.path?.length > 0 && node?.name?.length > 0) {
      this.contentPath = `${node.path}/${node.name}`;
      if (node?.type === ContentUnitType.File) {
        this.wikiViewer?.displayLocation(this.contentPath, true);
        rendered = true;
      } else if (node?.type === ContentUnitType.MarkdownIndexFolder) {
        this.wikiViewer?.displayLocation(`${this.contentPath}/index.md`, true);
        rendered = true;
      }
    }

    if (!rendered)
      this.wikiViewer?.reset();
  }
}