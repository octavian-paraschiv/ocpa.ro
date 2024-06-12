import { Ext } from 'src/environments/environment.ext';

export const environment = {
  production: true,
  apiUrl: location.origin + '/api',
  ext: new Ext(),
};
