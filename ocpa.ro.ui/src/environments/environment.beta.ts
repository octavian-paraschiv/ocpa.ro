import { Ext } from 'src/environments/environment.ext';

export const environment = {
  production: true,

  apiUrl: `${location.origin}/api-beta`,
  translationUrl: `${location.origin}/api-beta/content/render/translations/`,

  ext: new Ext(),
};
