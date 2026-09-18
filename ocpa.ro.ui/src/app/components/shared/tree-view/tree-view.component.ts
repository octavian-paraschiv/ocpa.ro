import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ContentUnit, ContentUnitType } from 'src/app/models/swagger/content-management';
import { NodeSelectedEvent } from '../content-tree/content-tree.component';
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
  @Output() nodeSelected = new EventEmitter<NodeSelectedEvent>();


  onTreeNodeSelected(event: NodeSelectedEvent) {
    this.selectNode(event);
  }

  toggle(node: ContentUnit) {
    if (node) {
      if (node.type === ContentUnitType.Folder ||
        node.type === ContentUnitType.MarkdownIndexFolder) {
        node.expanded = !node.expanded;
      }
    } 

    console.debug(`toggle calling selectNode: ${node?.path}\\${node?.name}`);
    this.selectNode({ node, isRefresh: false });
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

  selectNode(event: NodeSelectedEvent) {

    console.debug(`called selectNode: `);

    this.clearSelection();
    
    if (event?.node) {
      
      this.flattenTree(this.nodes).forEach(n => {
        const eventNodePath = this.normalizePath(`${event?.node?.path}/${event?.node?.name}`);
        const thisNodePath = this.normalizePath(`${n.path}/${n.name}`) ?? '';
        const isFolder = n.type === ContentUnitType.Folder || n.type === ContentUnitType.MarkdownIndexFolder;

        const isAncestor = isFolder && eventNodePath.startsWith(thisNodePath);

        if (isAncestor) {
          console.debug(` ${thisNodePath} is ancestor for ${eventNodePath}`);
          n.expanded = true;
        } 
      });
      
      event.node.selected = true;
      event.node.expanded = true;
    }      

    this.nodeSelected.emit(event);  
  }

  normalizePath = (path: string): string =>
  path
    .replace(/^[./\\]+(?=[^/\\])/, '') // remove leading "./" or ".\"
    .replace(/\\/g, '/');              // convert backslashes to slashes

  clearSelection() {
    console.debug(`called clearSelection`);
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