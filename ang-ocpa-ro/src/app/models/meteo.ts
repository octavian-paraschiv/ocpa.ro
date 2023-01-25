import { GridCoordinates } from "./geography";

export interface CalendarRange {
    start: string,
    end: string,
    length: number
}

export interface MeteoDailyData {
    date: string

    tMinActual?: number,
    tMaxActual?: number,
    tMinNormal?: number,
    tMaxNormal?: number,
    forecast?: string,
    tempFeel?: string,

    hazards?: string[],
    wind?: number,
    precip?: number,
    snowCover?: number,
    instability?: number,
    fog?: number,

    soilRain?: number,
    rain?: number,
    snow?: number,

    windDirection?: string

    p00?: number,
    p01?: number,
}

export interface MeteoData {
    gridCoordinates: GridCoordinates;
    calendarRange: CalendarRange;
    data: { [id: string]: MeteoDailyData };
}

export enum Unit 
{ 
    Metric,
    Imperial 
}