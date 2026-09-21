import { Component, Input, OnInit, Output, EventEmitter, ChangeDetectorRef, ViewChild, AfterViewInit } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { ContentUnit } from 'src/app/models/swagger/content-management';
import { ContentApiService } from 'src/app/services/api/content-api.service';
import { TreeViewComponent } from '../tree-view/tree-view.component';

@UntilDestroy()
@Component({
  selector: 'app-content-tree',
  templateUrl: 'content-tree.component.html'
})
export class ContentTreeComponent implements OnInit {
  @Input() path: string = '.';
  @Input() filter: string = '';
  @Input() level: number = 0;
  @Input() markdownView = false;
  @Input() initialLoad = true;
  @Output() nodeSelected = new EventEmitter<ContentUnit>();

  @ViewChild('treeView', { static: true }) treeView: TreeViewComponent;

  nodes: ContentUnit[] = [];
  
  constructor(private contentService: ContentApiService) {}

  ngOnInit(): void {
    if (this.initialLoad) {
      this.reloadAndSelect(undefined);
    }
  }
  
  onTreeNodeSelected(event: ContentUnit) {
    this.nodeSelected.emit(event);
  }

  reloadAndSelect(path: string | undefined) {
    this.contentService.listContent(this.path, this.level, this.filter, this.markdownView)
      .pipe(untilDestroyed(this))
      .subscribe(res => {
        this.treeView.nodes = res.children;

        const flatNodes = this.flattenTree(this.treeView.nodes);

        const parentPath = path?.substring(0, path.lastIndexOf('/'));

        let node = flatNodes.find(n => `${n.path}/${n.name}` === path) ??
          flatNodes.find(n => `${n.path}/${n.name}` === parentPath) ??
          flatNodes[0];

        this.treeView.selectNode(node);
      });
  }

  flattenTree(nodes: ContentUnit[]): ContentUnit[] {
    const flatArray: ContentUnit[] = [];
    const traverse = (node: ContentUnit) => {
      flatArray.push(node);
      if (node.children) node.children.forEach(traverse);
    };
    nodes.forEach(traverse);
    return flatArray.sort((a, b) => a?.path?.localeCompare(b?.path));
  }

  
}