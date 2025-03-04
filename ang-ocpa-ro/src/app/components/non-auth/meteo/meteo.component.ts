import { Component, ViewChild, OnInit } from '@angular/core';
import { MeteoDataBrowserComponent } from 'src/app/components/non-auth/meteo/meteo-data-browser/meteo-data-browser.component';

@Component({
  selector: 'app-meteo',
    templateUrl: './meteo.component.html'
})
export class MeteoComponent implements OnInit {
  @ViewChild('meteoDataBrowser', { static: true }) dataBrowser: MeteoDataBrowserComponent;

  ngOnInit(): void {
    this.dataBrowser.init();
  }
}
