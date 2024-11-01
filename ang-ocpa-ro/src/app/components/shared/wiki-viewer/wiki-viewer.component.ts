import {Component, Input, OnInit} from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { WikiService } from 'src/app/services/api-services';

@UntilDestroy()
@Component({
    selector: 'app-wiki-viewer',
    template: '<div [innerHtml]="content"></div>'
})
export class WikiViewerComponent implements OnInit {
    defaultContent = '<h5 style="text-align: justify;">Nothing here for now. Please stay tuned.</h5>';
    @Input() location: string | undefined;
    content = this.defaultContent;

    constructor(private wikiService: WikiService
    ) {
    }

    ngOnInit(): void {
        this.wikiService
            .getWiki(this.location)
            .pipe(untilDestroyed(this))
            .subscribe({
                next: res => {
                    this.content = res ?? this.defaultContent;
                },
                error: err => this.content = err.toString(),
            });
    }
}
