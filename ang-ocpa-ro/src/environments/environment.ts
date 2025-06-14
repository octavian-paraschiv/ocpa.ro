// This file can be replaced during build by using the `fileReplacements` array.
// `ng build ---prod` replaces `environment.ts` with `environment.prod.ts`.

import { Ext } from 'src/environments/environment.ext';

// The list of file replacements can be found in `angular.json`.
export const environment = {
  production: false,
  apiUrl: 'http://localhost:39207',
  translationUrl: './assets/translations/',

  //apiUrl: 'https://ocpa.ro/api',
  //translationUrl: 'https://ocpa.ro/api/content/render/translations/',

  ext: new Ext(),
};

/*
 * In development mode, to ignore zone related error stack frames such as
 * `zone.run`, `zoneDelegate.invokeTask` for easier debugging, you can
 * import the following file, but please comment it out in production mode
 * because it will have performance impact when throw error
 */
// import 'zone.js/plugins/zone-error';  // Included with Angular CLI.
