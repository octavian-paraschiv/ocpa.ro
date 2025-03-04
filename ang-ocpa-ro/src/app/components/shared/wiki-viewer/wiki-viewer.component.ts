import { Component, inject } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { TranslateService } from '@ngx-translate/core';
import { first } from 'rxjs/operators';
import { WikiService } from 'src/app/services/api/wiki.service';

@UntilDestroy()
@Component({
    selector: 'app-wiki-viewer',
    template: '<div [mathjax]="content"></div>'
})
export class WikiViewerComponent {
    content = 'n/a';

    private readonly translate = inject(TranslateService);
    private readonly wikiService = inject(WikiService);

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
