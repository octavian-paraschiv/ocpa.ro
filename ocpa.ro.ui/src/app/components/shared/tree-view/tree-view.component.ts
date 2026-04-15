import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ContentUnit, ContentUnitType } from 'src/app/models/swagger/content-management';
import { faFileText, faFolderOpen, faFolderClosed, faFileImage, faFile, faQuestion, faBook, faBookOpen } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-tree-view',
  templateUrl: './tree-view.component.html'
})
export class TreeViewComponent {
  folder = faFolderClosed;
  folderExpanded = faFolderOpen;
  markdownIndexFolder = faBook;
  markdownIndexFolderExpanded = faBookOpen;
  imageFile = faFileImage;
  textFile = faFileText;
  otherFile = faFile;
  unknown = faQuestion;
  size = 'grow-2';

  @Input({ required: true }) nodes: ContentUnit[];
  @Output() nodeSelected = new EventEmitter<ContentUnit>();


  onTreeNodeSelected(node: ContentUnit) {
    this.selectNode(node);
  }

  toggle(node: ContentUnit) {
    if (node) {
      if (node.type === ContentUnitType.Folder ||
        node.type === ContentUnitType.MarkdownIndexFolder) {
        node.expanded = !node.expanded;
      }
    } 
    this.selectNode(node);
  }

  nodeClass(node: ContentUnit) {
    if (node && node.type && node.type.length > 0)
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
        return node?.name?.toLowerCase().endsWith('.md') ? this.textFile :
          /\.(png|bmp|jpg|jpeg|gif)$/i.test(node?.name ?? '') ? this.imageFile : this.otherFile;
      default:
        return this.unknown;
    }
  }

  selectNode(node: ContentUnit) {
    this.clearSelection();
    
    if (node) 
      node.selected = true;

    this.nodeSelected.emit(node);  
  }

  clearSelection() {
    this.flattenTree(this.nodes).forEach(n => {
      n.selected = false;
      //n.expanded = false;
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