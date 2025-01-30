import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UntilDestroy } from '@ngneat/until-destroy';

export enum UnavailablePageKind {
    Unauthorized,
    NotFound
}

@UntilDestroy()
@Component({
    selector: 'app-unavailable-page',
    templateUrl: './unavailable-page.component.html'
})
export class UnavailablePageComponent {
    message = '';
    url = '';

    constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.route.url.subscribe(url => {
        if (!(this.url?.length > 0)) {
            this.url = `/${url.map(s => s.path).join('/')}`;
        }
    });
    this.route.queryParams.subscribe(params => {
        const kind = params['kind'];
        this.message = kind === UnavailablePageKind.Unauthorized.toString() ? 'unauthorized-access' : 'not-found';
        if (params['url']?.length > 0) {
            this.url = params['url'];
        }
    });
  }
}
