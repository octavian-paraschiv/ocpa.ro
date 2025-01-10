//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v14.1.0.0 (NJsonSchema v11.0.2.0 (Newtonsoft.Json v13.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

/* tslint:disable */
/* eslint-disable */
// ReSharper disable InconsistentNaming



export interface AppMenu {
    id?: number;
    name?: string | undefined;
    url?: string | undefined;
    displayMode?: EMenuDisplayMode;
    menuIcon?: string | undefined;
    userId?: number | undefined;
    appName?: string | undefined;
}

export interface Application {
    id?: number;
    name?: string | undefined;
    code?: string | undefined;
    loginRequired?: boolean;
    adminMode?: boolean;
    builtin?: boolean;
}

export interface ApplicationMenu {
    id?: number;
    applicationId?: number;
    menuId?: number;
}

export interface ApplicationUser {
    id?: number;
    applicationId?: number;
    userId?: number;
}

export interface AuthenticateResponse {
    loginId: string;
    token: string;
    type: number;
    expires: Date;
    validity: number;
}

export interface BuildInfo {
    title?: string | undefined;
    version?: BuildVersion;
    buildDate?: Date;
    isRelease?: boolean;
    comment?: string | undefined;
    url?: string | undefined;
}

export interface BuildVersion {
    major?: number;
    minor?: number;
    build?: number;
}

export interface CalendarRange {
    start?: Date;
    end?: Date;
    length?: number;
}

export interface CityDetail {
    id?: number;
    name?: string | undefined;
    regionId?: number;
    subregion?: string | undefined;
    lat?: number;
    lon?: number;
    default?: boolean;
    regionName?: string | undefined;
    regionCode?: string | undefined;
}

export interface ContentUnit {
    type?: ContentUnitType;
    name?: string | undefined;
    path?: string | undefined;
    size?: number;
    children?: ContentUnit[] | undefined;
}

export enum ContentUnitType {
    None = "None",
    Folder = "Folder",
    File = "File",
}

export enum EMenuDisplayMode {
    AlwaysHide = "AlwaysHide",
    AlwaysShow = "AlwaysShow",
    HideOnMobile = "HideOnMobile",
    ShowOnMobile = "ShowOnMobile",
}

export interface GeoLocation {
    status?: string | undefined;
    continent?: string | undefined;
    continentCode?: string | undefined;
    country?: string | undefined;
    countryCode?: string | undefined;
    region?: string | undefined;
    regionName?: string | undefined;
    city?: string | undefined;
    district?: string | undefined;
    zip?: string | undefined;
    lat?: number;
    lon?: number;
    timezone?: string | undefined;
    offset?: number;
    currency?: string | undefined;
    isp?: string | undefined;
    org?: string | undefined;
    as?: string | undefined;
    asname?: string | undefined;
    reverse?: string | undefined;
    mobile?: boolean;
    proxy?: boolean;
    hosting?: boolean;
}

export interface GridCoordinates {
    r?: number;
    c?: number;
}

export interface Lab {
    id?: number;
    code?: string | undefined;
    description?: string | undefined;
    comment?: string | undefined;
}

export interface Menu {
    id?: number;
    name?: string | undefined;
    url?: string | undefined;
    displayModeId?: number;
    menuIcon?: string | undefined;
    builtin?: boolean;
}

export interface Menus {
    publicMenus?: VMenu[] | undefined;
    appMenus?: AppMenu[] | undefined;
    deviceId?: string | undefined;
}

export interface MeteoDailyData {
    date?: string | undefined;
    tMinActual?: number;
    tMaxActual?: number;
    tMinNormal?: number;
    tMaxNormal?: number;
    forecast?: string | undefined;
    tempFeel?: string | undefined;
    hazards?: string[] | undefined;
    wind?: number;
    precip?: number;
    snowCover?: number;
    soilRain?: number;
    instability?: number;
    fog?: number;
    rain?: number;
    snow?: number;
    windDirection?: string | undefined;
    p00?: number;
    p01?: number;
}

export interface MeteoData {
    name?: string | undefined;
    dbi?: number;
    calendarRange?: CalendarRange;
    readonly dataCount?: number;
    readonly online?: boolean;
    readonly modifyable?: boolean;
    gridCoordinates?: GridCoordinates;
    data?: { [key: string]: MeteoDailyData; } | undefined;
}

export interface MeteoDbInfo {
    name?: string | undefined;
    dbi?: number;
    calendarRange?: CalendarRange;
    readonly dataCount?: number;
    readonly online?: boolean;
    readonly modifyable?: boolean;
}

export interface Person {
    id?: number;
    cnp?: string | undefined;
    name?: string | undefined;
    comment?: string | undefined;
}

export interface RegisteredDevice {
    id?: number;
    deviceId?: string | undefined;
    lastLoginId?: string | undefined;
    lastLoginTimestamp?: Date;
    lastLoginIpAddress?: string | undefined;
    lastLoginGeoLocation?: string | undefined;
}

export interface SensorData {
    temperature?: number;
    pressure?: number;
    humidity?: number;
}

export interface SensorDataCollection {
    sensorData?: SensorData[] | undefined;
}

export interface Test {
    id?: number;
    labId?: number;
    personId?: number;
    testTypeId?: number;
    date?: Date;
    value?: number | undefined;
    minRefOverride?: number | undefined;
    maxRefOverride?: number | undefined;
    description?: string | undefined;
    comment?: string | undefined;
}

export interface TestCategory {
    id?: number;
    code?: string | undefined;
    description?: string | undefined;
    comment?: string | undefined;
}

export interface TestDetail {
    testId?: number;
    personId?: number;
    testTypeId?: number;
    testCategoryId?: number;
    labId?: number;
    personName?: string | undefined;
    personCode?: string | undefined;
    personComment?: string | undefined;
    testCategoryCode?: string | undefined;
    testCategoryDescription?: string | undefined;
    testTypeCode?: string | undefined;
    testTypeDescription?: string | undefined;
    minRef?: number;
    maxRef?: number;
    description?: string | undefined;
    date?: Date;
    value?: number | undefined;
    minRefOverride?: number | undefined;
    maxRefOverride?: number | undefined;
}

export interface TestSearchRequest {
    id?: number | undefined;
    pid?: number | undefined;
    cnp?: string | undefined;
    category?: string | undefined;
    type?: string | undefined;
    from?: Date | undefined;
    to?: Date | undefined;
}

export interface TestType {
    id?: number;
    testCategoryId?: number;
    code?: string | undefined;
    minRef?: number;
    maxRef?: number;
    description?: string | undefined;
    comment?: string | undefined;
}

export interface TestTypeDetail {
    testTypeId?: number;
    testCategoryId?: number;
    testCategoryCode?: string | undefined;
    testCategoryDescription?: string | undefined;
    testTypeCode?: string | undefined;
    testTypeDescription?: string | undefined;
    minRef?: number;
    maxRef?: number;
}

export interface User {
    id?: number;
    loginId?: string | undefined;
    passwordHash?: string | undefined;
    type?: number;
    enabled?: boolean;
}

export interface UserType {
    id?: number;
    code?: string | undefined;
    description?: string | undefined;
}

export interface VMenu {
    id?: number;
    name?: string | undefined;
    url?: string | undefined;
    displayMode?: EMenuDisplayMode;
    menuIcon?: string | undefined;
}

export interface Body {
    LoginId: string;
    Password: string;

    [key: string]: any;
}