import { ViewChild } from '@angular/core';
import { Component, OnInit, OnChanges } from '@angular/core';
import { ActivatedRoute, ActivationStart, Router } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { filter, map } from 'rxjs/operators';
import { WikiViewerComponent } from 'src/app/components/shared/wiki-viewer/wiki-viewer.component';

@UntilDestroy()
@Component({
    selector: 'app-wiki-container',
    templateUrl: './wiki-container.component.html'
})
export class WikiContainerComponent implements OnInit {
  @ViewChild('viewer', { static: true }) dataBrowser: WikiViewerComponent;

  constructor(private readonly route: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.route.params
      .pipe(untilDestroyed(this))
      .subscribe(() => this.reloadLocation());
  }

  reloadLocation() {
    try { 
      const url = this.route?.snapshot?.url;
      let location = url.map(s => s.path).join('/').replace('wiki-container/', '');
      if (!location.endsWith('.md'))
        location = location + '/index.md';
      this.dataBrowser?.displayLocation(location);
    }                
    catch { }
  }
}
