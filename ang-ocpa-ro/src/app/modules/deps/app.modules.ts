import { HttpClient, HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { NgSelectModule } from '@ng-select/ng-select';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { MathjaxModule } from 'mathjax-angular';
import { NgChartsModule } from 'ng2-charts';
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
    MathjaxModule.forRoot({
      config: {
        loader: { load: ['input/tex', 'output/svg'] },
        tex: { inlineMath: [['$', '$'], ['\\(', '\\)']] },
        svg: { fontCache: 'global' }
      },
      src: 'https://cdn.jsdelivr.net/npm/mathjax@3/es5/startup.js'
    }),
    BrowserModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    NoopAnimationsModule,
    FontAwesomeModule,
    NgSelectModule,
    MaterialModule,
    NgChartsModule
];