import { GridCoordinates } from "./geography";

export interface CalendarRange {
    start: string,
    end: string,
    length: number
}

export interface MeteoDailyData {
    tMinActual: number,
    tMaxActual: number,
    tMinNormal: number,
    tMaxNormal: number,
    forecast: string,
    tempFeel: string
}

export interface MeteoData {
    gridCoordinates: GridCoordinates;
    calendarRange: CalendarRange;
    data: { [id: string]: MeteoDailyData };
}

export interface MeteoDailyDataEx extends MeteoDailyData {
    date: string
}