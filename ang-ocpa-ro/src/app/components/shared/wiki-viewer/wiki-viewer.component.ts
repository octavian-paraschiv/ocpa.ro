import {Component, Input, OnInit} from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { TranslateService } from '@ngx-translate/core';
import { WikiService } from 'src/app/services/api-services';

@UntilDestroy()
@Component({
    selector: 'app-wiki-viewer',
    template: '<div [innerHtml]="content"></div>'
})
export class WikiViewerComponent implements OnInit {
    @Input() location: string | undefined;
    content = 'n/a';

    constructor(
        private translate: TranslateService,
        private wikiService: WikiService
    ) {
    }

    ngOnInit(): void {
        this.wikiService
            .getWiki(this.location)
            .pipe(untilDestroyed(this))
            .subscribe({
                next: res => this.content = res ?? this.translate.instant('wiki.default-content'),
                error: err => this.content = err.toString(),
            });
    }
}
