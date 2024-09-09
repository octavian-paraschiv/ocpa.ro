import { Component, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { BaseAuthComponent } from 'src/app/components/auth/base/BaseAuthComponent';
import { ContentApiService, MeteoApiService } from 'src/app/services/api-services';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { take } from 'rxjs/operators';
import { ContentUnit } from 'src/app/models/models-swagger';
import { FlatTreeControl } from '@angular/cdk/tree';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material/tree';
import { faFolder, faFolderOpen, faFile, faFileLines } from '@fortawesome/free-solid-svg-icons';

/** Flat node with expandable and level information */
interface ExampleFlatNode {
    expandable: boolean;
    name: string;
    level: number;
}

@UntilDestroy()
@Component({
    selector: 'app-meteo-database',
    templateUrl: './meteo-database.component.html'
})
export class MeteoDatabaseComponent extends BaseAuthComponent {
    faFolder = faFolder;
    faFolderOpen = faFolderOpen;
    faFile = faFileLines;

    studioDownloadUrl: string = undefined;

    private _transformer = (node: ContentUnit, level: number) => {
        return {
          expandable: !!node.children && node.children.length > 0,
          name: node.name,
          level: level,
        };
      };
    
    
    treeControl = new FlatTreeControl<ExampleFlatNode>(
        node => node.level,
        node => node.expandable,
    );

    treeFlattener = new MatTreeFlattener(
        this._transformer,
        node => node.level,
        node => node.expandable,
        node => node.children,
      );

    dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);

    constructor(
        router: Router,
        authenticationService: AuthenticationService,
        ngZone: NgZone,
        private readonly meteoApi: MeteoApiService,
        private readonly contentApi: ContentApiService,
    ) { 
        super(router, authenticationService, ngZone);
    }

    protected onInit(): void {
        this.meteoApi.getStudioDownloadUrl()
            .pipe(take(1), untilDestroyed(this))
            .subscribe(url => this.studioDownloadUrl = url);

        this.contentApi.getContent()
            .pipe(take(1), untilDestroyed(this))
            .subscribe(ct => {
                this.dataSource.data = [ct];
                this.treeControl.expand(this.treeControl.dataNodes[0]);
            });
    }

    hasChild = (_: number, node: ExampleFlatNode) => node.expandable;

    folderIcon = (node: ExampleFlatNode) => this.treeControl.isExpanded(node) ? faFolderOpen : faFolder;
}
