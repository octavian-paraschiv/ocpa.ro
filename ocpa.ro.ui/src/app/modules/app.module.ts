import { NgModule } from '@angular/core';
import { provideMarkdown } from 'ngx-markdown';
import { AppComponent } from 'src/app/components/app.component';
import { components } from 'src/app/modules/deps/app.components';
import { directives } from 'src/app/modules/deps/app.directives';
import { initializers } from 'src/app/modules/deps/app.initializers';
import { modules } from 'src/app/modules/deps/app.modules';
import { pipes } from 'src/app/modules/deps/app.pipes';
import { services } from 'src/app/modules/deps/app.services';

@NgModule({
  declarations: [
      components,
      pipes,
      directives
  ],

  imports: [ 
    modules,
  ],

  providers: [
    initializers,
    services,
    provideMarkdown()
  ],

  bootstrap: [
    AppComponent
  ]
})
export class AppModule { }
