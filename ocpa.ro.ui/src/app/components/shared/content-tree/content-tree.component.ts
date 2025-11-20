import { ITreeOptions, TreeComponent, TreeModel, TreeNode } from '@ali-hm/angular-tree-component';
import { TREE_ACTIONS, TreeOptions } from '@ali-hm/angular-tree-component/lib/models/tree-options.model';
import { Component, Input, OnInit, Output, EventEmitter, ChangeDetectorRef, ViewChild } from '@angular/core';
import { faFileText, faFolderOpen, faFolderClosed, faFileImage, faFile, faQuestion, faBook, faBookOpen } from '@fortawesome/free-solid-svg-icons';
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
  @Input() filter?: string;
  @Input() level: number = 0;
  @Input() markdownView = false;
  @Input() initialLoad = true;
  @Output() nodeSelected = new EventEmitter<ContentUnit>();

  @ViewChild('treeView', { static: true }) treeView: TreeComponent;

  nodes: ContentUnit[] = [];
  
  options: ITreeOptions = {
    useCheckbox: false,
    displayField: 'name',
    idField: 'path',
    isExpandedField: 'expanded',
    hasChildrenField: 'children',
    useVirtualScroll: false,

    actionMapping: {
      mouse: {
        click: (tree : TreeModel, node: TreeNode, event: Event) => {
          this.selectNode(node.data);
    		}
      }
    }

  };

  folder = faFolderClosed;
  folderExpanded = faFolderOpen;
  markdownIndexFolder = faBook;
  markdownIndexFolderExpanded = faBookOpen;
  imageFile = faFileImage;
  textFile = faFileText;
  otherFile = faFile;
  unknown = faQuestion;

  constructor(private contentService: ContentApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    if (this.initialLoad) {
      this.reloadAndSelect(undefined);
    }
  }

  reloadAndSelect(path: string) {
    let id = 1;
    this.contentService.listContent(this.path, this.level, this.filter, this.markdownView)
      .pipe(untilDestroyed(this))
      .subscribe(res => {
        this.nodes = res.children;
        this.treeView.treeModel.collapseAll();
        const flatNodes = this.flattenTree(this.nodes);
        flatNodes.forEach(node => {
          node.selected = false;
          node.expanded = false;
        });
        const node = flatNodes.find(n => `${n.path}/${n.name}` === path) ?? flatNodes[0];
        this.selectNode(node);
      });
  }

  selectNode(node: ContentUnit) {
    this.flattenTree(this.nodes).forEach(n => {
      n.selected = false;
      n.expanded = false;
    });

    if (node) {
      node.selected = true;
      this.nodeSelected.emit(node);
    }
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

  nodeClass(node: ContentUnit) {
    if (node?.type?.length > 0)
      return `node-${node.type}`;
    return 'node-None';
  }

  icon(node: ContentUnit) {
    switch (node?.type) {
      case ContentUnitType.Folder:
        return node.expanded ? this.folderExpanded : this.folder;
      case ContentUnitType.MarkdownIndexFolder:
        return node.expanded ? this.markdownIndexFolderExpanded : this.markdownIndexFolder;
      case ContentUnitType.File:
        return node.name.toLowerCase().endsWith('.md') ? this.textFile :
          /\.(png|bmp|jpg|jpeg|gif)$/i.test(node.name) ? this.imageFile : this.otherFile;
      default:
        return this.unknown;
    }
  }
}