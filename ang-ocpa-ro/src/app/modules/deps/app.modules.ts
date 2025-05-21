import { HttpClient, HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { GoogleMapsModule } from '@angular/google-maps';
import { BrowserModule } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { NgSelectModule } from '@ng-select/ng-select';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { NgChartsModule } from 'ng2-charts';
import { MarkdownModule } from 'ngx-markdown';
import { routes } from 'src/app/modules/deps/app.routes';
import { MaterialModule } from 'src/app/modules/material.module';

const translationConfig = {
    defaultLanguage: 'en',
    loader: {
      provide: TranslateLoader,
      useFactory: (httpClient: HttpClient) => new TranslateHttpLoader(httpClient, './assets/translations/', '.json'),
      deps: [HttpClient]
    }
  }
  
export const modules = [
    // Modules
    TranslateModule.forRoot(translationConfig),
    RouterModule.forRoot(routes),
    BrowserModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    NoopAnimationsModule,
    FontAwesomeModule,
    NgSelectModule,
    MaterialModule,
    NgChartsModule,
    MarkdownModule.forRoot(),
    GoogleMapsModule
];