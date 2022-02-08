export interface CalendarRange {
  Start: string;
  End: string;
  Length: number;
}

export interface MeteoDailyData
{
    TMinActual: string;
    TMaxActual: string;
    TMinNormal: string;
    TMaxNormal: string;
    Forecast: string;
    TempFeel: string;
}

export interface MeteoData {
    CalendarRange: CalendarRange;
    Data: { [id: string]: MeteoDailyData }[];
}
