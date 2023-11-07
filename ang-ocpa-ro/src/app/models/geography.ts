export interface City {
    name?: string,
    region?: string,
    subregion?: string,
    latitude?: number,
    longitude?: number,
    default?: boolean
}

export interface GridCoordinates {
    r: number,
    c: number
}

export interface IsoCountry {
    IsoNumeric?: string;
    IsoAlpha2?: string;
    IsoAlpha3?: string;
    CountryName?: string;
    OfficialCountryName?: string;
    CountryNativeName?: string;
    OfficialCountryNativeName?: string;
    Demonym1?: string;
    Demonym2?: string;
    Demonym3?: string;
    PhoneCode?: string;
  }
  