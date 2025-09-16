import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { IsoCountry } from 'src/app/models/models-local';

@Injectable()
@UntilDestroy()
export class Iso3166HelperService {
  public countries: IsoCountry[];

  constructor(private httpClient: HttpClient) {
  }

  public init(): Observable<boolean> {
    try {
      return this.httpClient.get('assets/iso3166.json')
        .pipe(
          untilDestroyed(this),
          map(data => {
            try {
              this.countries = data as IsoCountry[];
              return true;

            } catch (err) {
              return false;
            }
          }));

    } catch (err) {
      return of(false);
    }
  }

  public getByCountryCode(inputCode: string): IsoCountry {
    const code = (inputCode ?? '').toLocaleUpperCase();
    return this.countries?.find(x => (x.IsoAlpha3 === code || x.IsoAlpha2 === code));
  }

  public getByCountryName(input: string): IsoCountry {
    const name = (input ?? '').toLocaleUpperCase();
    return this.countries?.find(x => (x.CountryName.toLocaleUpperCase() === name));
  }
}
