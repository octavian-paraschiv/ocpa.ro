export enum Unit { 
    Metric,
    Imperial 
}

export enum UnitType { 
    Temperature,
    Pressure,
    Distance
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
