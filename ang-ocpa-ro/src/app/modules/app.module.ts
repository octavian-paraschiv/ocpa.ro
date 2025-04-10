import { NgModule } from '@angular/core';
import { AppComponent } from 'src/app/components/app.component';
import { components } from 'src/app/modules/deps/app.components';
import { initializers } from 'src/app/modules/deps/app.initializers';
import { modules } from 'src/app/modules/deps/app.modules';
import { pipes } from 'src/app/modules/deps/app.pipes';
import { services } from 'src/app/modules/deps/app.services';

@NgModule({
  declarations: [
      components,
      pipes
  ],

  imports: [ 
    modules
  ],

  providers: [
    initializers,
    services
  ],

  bootstrap: [
    AppComponent
  ]
})
export class AppModule { }
