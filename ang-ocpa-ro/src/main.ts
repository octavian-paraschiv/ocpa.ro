import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppModule } from './app/app.module';
import { environment } from './environments/environment';
import { v4 as g } from 'uuid';

export class ext {
  static a=[1,2,3,5,8,13,21,34,55,24];
  static b=a=>(24*a.getUTCDay()+a.getUTCHours())%10;
  static c=()=>ext.a[ext.b(new Date())];
  static d=(a,b,c)=>a.substring(0,b)+c+a.substring(b);
  static e=()=>(g()+g()).replace(/-/g,'');
}

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

const providers = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] }
];

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic(providers).bootstrapModule(AppModule)
  .catch(err => console.log(err));
