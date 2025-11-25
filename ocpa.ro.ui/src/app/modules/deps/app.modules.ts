import { TreeModule } from '@ali-hm/angular-tree-component';
import { HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { GoogleMapsModule } from '@angular/google-maps';
import { BrowserModule } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { NgSelectModule } from '@ng-select/ng-select';
import { TranslateModule } from '@ngx-translate/core';
import { MarkdownModule } from 'ngx-markdown';
import { BootstrapModule } from 'src/app/modules/bootstrap.module';
import { routes } from 'src/app/modules/deps/app.routes';
import { translationConfig } from 'src/app/services/translation-init.service';
import { GoogleChartsModule } from 'angular-google-charts';
import { ToastrModule } from 'ngx-toastr';

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
    GoogleChartsModule,
    MarkdownModule.forRoot(),
    GoogleMapsModule,
    BootstrapModule,
    TreeModule,

    ToastrModule.forRoot()
    //ToastNoAnimationModule.forRoot(),
];