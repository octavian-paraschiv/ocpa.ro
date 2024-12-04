import { Component, ViewChild } from '@angular/core';
import { TranslateCompiler, TranslateService } from '@ngx-translate/core';
import { BaseLifecycleComponent } from 'src/app/components/BaseLifecycleComponent';
import { MeteoDataBrowserComponent } from 'src/app/components/non-auth/meteo/meteo-data-browser/meteo-data-browser.component';

@Component({
  selector: 'app-meteo',
    templateUrl: './meteo.component.html'
})
export class MeteoComponent extends BaseLifecycleComponent {
  @ViewChild('meteoDataBrowser', { static: true }) dataBrowser: MeteoDataBrowserComponent;
  constructor(translate: TranslateService) {
    super(translate);
  }

  protected onInit(): void {
    this.dataBrowser.init();
  }
}
