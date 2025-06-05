import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { WikiViewerComponent } from 'src/app/components/shared/wiki-viewer/wiki-viewer.component';

@UntilDestroy()
@Component({
    selector: 'app-wiki-container',
    templateUrl: './wiki-container.component.html'
})
export class WikiContainerComponent implements OnInit {
  @ViewChild('viewer', { static: true }) dataBrowser: WikiViewerComponent;

  private readonly route = inject(ActivatedRoute);

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
      this.dataBrowser?.displayLocation(`wiki/${location}`, true);
    }                
    catch { }
  }
}
