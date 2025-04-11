import { NestedTreeControl } from '@angular/cdk/tree';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { MatTreeNestedDataSource } from '@angular/material/tree';
import { faFileText } from '@fortawesome/free-regular-svg-icons';
import { faFolderOpen, faFolderClosed, faFileImage, faFile, faQuestion } from '@fortawesome/free-solid-svg-icons';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { TranslateService } from '@ngx-translate/core';
import { ContentUnit, ContentUnitType } from 'src/app/models/models-swagger';
import { ContentApiService } from 'src/app/services/api/content-api.service';

@UntilDestroy()
@Component({
  selector: 'app-content-tree',
  templateUrl: 'content-tree.component.html'
})
export class ContentTreeComponent implements OnInit {
  @Input() path?: string = '.';
  @Input() filter?: string = undefined;
  @Input() level?: number = undefined;
  @Output() nodeSelected = new EventEmitter<ContentUnit>();

  folder = faFolderClosed;
  folderExpanded = faFolderOpen;
  imageFile = faFileImage;
  textFile = faFileText;
  otherFile = faFile;
  unknown = faQuestion;

  size = "shrink-6";

  treeControl = new NestedTreeControl<ContentUnit>(node => node.children);
  dataSource = new MatTreeNestedDataSource<ContentUnit>();

  selectedNode: ContentUnit = undefined;

  constructor(private translate: TranslateService,
    private contentService: ContentApiService
  ) {
  }

  ngOnInit(): void {
    this.contentService.listContent(this.path, this.level, this.filter)
      .pipe(untilDestroyed(this))
      .subscribe(res => this.dataSource.data = res.children);
  }

  select(node: ContentUnit) {
    if (node === this.selectedNode)
      this.selectedNode = undefined;
    else {
      this.selectedNode = node;
      this.nodeSelected.emit(node);
    }
  }

  isSelected = (node: ContentUnit) => 
    node?.name === this.selectedNode?.name && 
    node?.path === this.selectedNode?.path;

  class = (node: ContentUnit) => this.isSelected(node) ? 'mat-tree-node warmer' : 'mat-tree-node'

  hasChild = (_: number, node: ContentUnit) => 
    node?.children?.length > 0;

    icon(node: ContentUnit) {
    switch(node?.type) {
      case ContentUnitType.Folder:
        return this.treeControl.isExpanded(node) ? this.folderExpanded : this.folder;

      case ContentUnitType.File:
        return node?.name?.toLocaleLowerCase()?.endsWith('.md') ? this.textFile :
          (node?.name?.toLocaleLowerCase()?.endsWith('.png') || 
          node?.name?.toLocaleLowerCase()?.endsWith('.bmp') || 
          node?.name?.toLocaleLowerCase()?.endsWith('.jpg') || 
          node?.name?.toLocaleLowerCase()?.endsWith('.jpeg') || 
          node?.name?.toLocaleLowerCase()?.endsWith('.gif')) ? this.imageFile : 
            this.otherFile;

      default:
        return this.unknown;
    }
  }
}
