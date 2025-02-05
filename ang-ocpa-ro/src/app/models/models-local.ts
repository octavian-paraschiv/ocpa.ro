import { Application, User, ApplicationUser } from 'src/app/models/models-swagger';

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

export interface AppMenuData {
    menuId: number;
    menuName: string;
    appData: AppData[];
}

export interface AppData {
    appName: string;
    appId: number;
    active: boolean;
}

export interface ApplicationInfo extends Application {
    selected: boolean;
}

export interface UserInfo extends User {
    appsForUser: ApplicationUser[];
}

export interface MessageBoxOptions {
    message: string;
    title: string;
    icon: string;
    yesButtonText?: string;
    noButtonText?: string;
    noTimeout?: number;
    yesTimeout?: number;
    isSessionTimeoutMessage?: boolean;
}
