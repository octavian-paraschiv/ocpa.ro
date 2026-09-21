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


  onTreeNodeSelected(event: ContentUnit) {
    this.selectNode(event);
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

  selectNode(event: ContentUnit) {
    this.clearSelection();
    
    if (event) {
      this.flattenTree(this.nodes).forEach(n => {
        const eventNodePath = this.normalizePath(`${event?.path}/${event?.name}`);
        const thisNodePath = this.normalizePath(`${n.path}/${n.name}`) ?? '';
        const isFolder = n.type === ContentUnitType.Folder || n.type === ContentUnitType.MarkdownIndexFolder;

        // Expand node if it is an ancestor of the node to be selected
        n.expanded = isFolder && eventNodePath.startsWith(thisNodePath);
      });
      
      event.selected = true;
      event.expanded = true;
    }      

    this.nodeSelected.emit(event);  
  }

  normalizePath = (path: string): string =>
  path
    .replace(/^[./\\]+(?=[^/\\])/, '') // remove leading "./" or ".\"
    .replace(/\\/g, '/');              // convert backslashes to slashes

  clearSelection() {
    this.flattenTree(this.nodes).forEach(n => n.selected = false);
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