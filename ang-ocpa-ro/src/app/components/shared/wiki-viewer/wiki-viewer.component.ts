import {Component} from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { TranslateService } from '@ngx-translate/core';
import { WikiService } from 'src/app/services/api-services';
import { first } from 'rxjs/operators';

@UntilDestroy()
@Component({
    selector: 'app-wiki-viewer',
    template: '<div [innerHtml]="content"></div>'
})
export class WikiViewerComponent {
    content = 'n/a';
    constructor(
        private translate: TranslateService,
        private wikiService: WikiService
    ) {
    }

    public displayLocation(location: string) {
        this.wikiService
        .getWiki(location)
        .pipe(first(), untilDestroyed(this))
        .subscribe({
            next: res => this.content = res ?? this.translate.instant('wiki.default-content'),
            error: err => this.content = err.toString(),
        });
    }
}
