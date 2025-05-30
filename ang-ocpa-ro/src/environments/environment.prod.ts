import { Ext } from 'src/environments/environment.ext';

export const environment = {
  production: true,

  apiUrl: `${location.origin}/api`,
  translationUrl: `${location.origin}/api/content/render/translations/`,

  ext: new Ext(),
};
