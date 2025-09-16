import { NestedTreeControl } from '@angular/cdk/tree';
import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { MatTreeNestedDataSource } from '@angular/material/tree';
import { faFileText } from '@fortawesome/free-regular-svg-icons';
import { faFolderOpen, faFolderClosed, faFileImage, faFile, faQuestion, faFileCircleXmark, faFileContract, faBook, faBookOpen } from '@fortawesome/free-solid-svg-icons';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { ContentUnit, ContentUnitType } from 'src/app/models/models-swagger';
import { ContentApiService } from 'src/app/services/api/content-api.service';

@UntilDestroy()
@Component({
  selector: 'app-content-tree',
  templateUrl: 'content-tree.component.html'
})
export class ContentTreeComponent implements OnInit {
  @Input() path: string = '.';
  @Input() filter: string = undefined;
  @Input() level: number = 0;
  @Input() markdownView: boolean = false;

  @Input() initialLoad = true;
  @Output() nodeSelected = new EventEmitter<ContentUnit>();

  folder = faFolderClosed;
  folderExpanded = faFolderOpen;

  markdownIndexFolder = faBook;
  markdownIndexFolderExpanded = faBookOpen;

  imageFile = faFileImage;
  textFile = faFileText;
  otherFile = faFile;
  unknown = faQuestion;

  size = "grow-2";

  treeControl = new NestedTreeControl<ContentUnit>(node => node.children);
  dataSource = new MatTreeNestedDataSource<ContentUnit>();

  constructor(private contentService: ContentApiService,
    private cdr: ChangeDetectorRef
  ) {
  }

  ngOnInit(): void {
    if (!!this.initialLoad) {
      this.reloadAndSelect(undefined);
    }
  }

  reloadAndSelect(path: string) {
    this.contentService.listContent(this.path, this.level, this.filter, this.markdownView)
      .pipe(untilDestroyed(this))
      .subscribe(res => {
        this.dataSource.data = res.children;
        const nodes = this.flattenTree(res.children);
        const node = nodes.find(node => `${node.path}/${node.name}` === path) ?? nodes[0];
        this.selectAndExpandNode(node);
      });
  }

  select(node: ContentUnit) {
    this.flattenTree(this.dataSource.data).forEach(n => n.selected = false);
    if (node) {
      node.selected = true;
    }
    this.nodeSelected.emit(node);
  }
  
  flattenTree(nodes: ContentUnit[]): ContentUnit[] {
    let flatArray: ContentUnit[] = [];
  
    function traverse(node: ContentUnit) {
      flatArray.push(node);
      if (node.children) {
        node.children.forEach(child => traverse(child));
      }
    }
  
    nodes.forEach(node => traverse(node));
    return flatArray.sort((n1, n2) => n1?.path?.localeCompare(n2?.path));
  }  
  
  selectAndExpandNode(node: ContentUnit) {
    this.flattenTree(this.dataSource.data).forEach(node => node.selected = false);
    if (node) {
      const path = this.findNode(this.dataSource.data, node);
      if (path) {
        path.forEach(n => this.treeControl.expand(n));
        node.selected = true;
        this.nodeSelected.emit(node);
        this.cdr.detectChanges(); // Ensure change detection picks up the changes
      }
    }
  }
  
  findNode(nodes: ContentUnit[], targetNode: ContentUnit, path: ContentUnit[] = []): ContentUnit[] | null {
    for (let node of nodes) {
      const currentPath = [...path, node];
      if (node === targetNode) {
        return currentPath;
      }
      if (node.children) {
        const found = this.findNode(node.children, targetNode, currentPath);
        if (found) {
          return found;
        }
      }
    }
    return null;
  }

  hasChild = (_: number, node: ContentUnit) => node?.children?.length > 0;

  icon(node: ContentUnit) {
    switch(node?.type) {
      case ContentUnitType.Folder:
        return this.treeControl.isExpanded(node) ? this.folderExpanded : this.folder;

      case ContentUnitType.MarkdownIndexFolder:
        return this.treeControl.isExpanded(node) ? this.markdownIndexFolderExpanded : this.markdownIndexFolder;

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
