import { Component, OnInit } from '@angular/core';
import { take } from 'rxjs/operators';
import { BuildInfo } from 'src/app/models/protone';
import { ProtoneApiService } from 'src/app/services/api-services';

@Component({
    selector: 'app-protone',
    templateUrl: './protone.component.html',
    styleUrls: ['../../../assets/styles/site.css']
})
export class ProTONEComponent implements OnInit {

    latestBuild: BuildInfo = undefined;
    legacyBuilds: BuildInfo[] = []
    developmentBuilds: BuildInfo[] = [];

    constructor(private readonly api: ProtoneApiService) {
    }

    ngOnInit(): void {
        this.api.getProtoneBuilds('legacy')
            .pipe(take(1))
            .subscribe(builds => {
                this.legacyBuilds = this.orderByVersion(builds);
            });

        this.api.getProtoneBuilds('true')
            .pipe(take(1))
            .subscribe(builds => {
                if (builds && builds.length > 0) {
                    this.latestBuild = this.orderByVersion(builds)[0];
                }
            });

        this.api.getProtoneBuilds('false')
            .pipe(take(1))
            .subscribe(builds => {
                this.developmentBuilds = builds;
            });
    }

    private orderByVersion(builds: BuildInfo[]): BuildInfo[] {
        if (builds && builds.length > 0) {
            return builds.sort((bi1, bi2) =>
                1000000 * (bi2.version.major - bi1.version.major) +
                1000 * (bi2.version.minor - bi1.version.minor) +
                1 * (bi2.version.build - bi1.version.build));
        }

        return [];
    }
}
