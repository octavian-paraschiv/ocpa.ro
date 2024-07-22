import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, of } from "rxjs";
import { environment } from "src/environments/environment";
import { City, GridCoordinates } from "../models/geography";
import { CalendarRange, MeteoData } from "../models/meteo";
import { BuildInfo  } from "../models/protone";
import { UntilDestroy, untilDestroyed } from "@ngneat/until-destroy";
import { catchError, map } from "rxjs/operators";

@UntilDestroy()
@Injectable()
export class ProtoneApiService {
    constructor(private readonly httpClient: HttpClient) {
    }

    public getProtoneBuilds(type: string): Observable<BuildInfo[]> {
        const uri = `${environment.apiUrl}/protone?release=${type}`;
        return this.httpClient.get<BuildInfo[]>(uri);
    }
}

@UntilDestroy()
@Injectable()
export class GeographyApiService {

    private _cities: City[] = [];

    constructor(private readonly httpClient: HttpClient) {
    }

    public init(): Observable<boolean> {
        console.debug('Calling GeographyApiService.init...');
        return this.getAllCities()
            .pipe(
                untilDestroyed(this),
                map(cities => {
                    this._cities = cities;
                    return true;
                }),
                catchError(err => {
                    console.error(err);
                    this._cities = [];
                    return of(false);
                })
            )
            
    }

    public getRegions(): Observable<string[]> {
        const uri = `${environment.apiUrl}/geography/regions`;
        return this.httpClient.get<string[]>(uri);
    }

    public getSubregions(region: string): Observable<string[]> {
        const uri = `${environment.apiUrl}/geography/subregions?region=${region}`;
        return this.httpClient.get<string[]>(uri);
    }

    public getCities(region: string, subregion: string): Observable<string[]> {
        const uri = `${environment.apiUrl}/geography/cities?region=${region}&subregion=${subregion}`;
        return this.httpClient.get<string[]>(uri);
    }

    public getCityInfo(region: string, subregion: string, city: string): Observable<City> {
        const uri = `${environment.apiUrl}/geography/city?region=${region}&subregion=${subregion}&city=${city}`;
        return this.httpClient.get<City>(uri);
    }

    public getGridCoordinates(region: string, subregion: string, city: string): Observable<GridCoordinates> {
        const uri = `${environment.apiUrl}/geography/grid?region=${region}&subregion=${subregion}&city=${city}`;
        return this.httpClient.get<GridCoordinates>(uri);
    }

    public get cities(): City[] {
        return GeographyApiService.SortCities([], this._cities);
    }

    public static SortCities(terms: string[], cities: City[]): City[] {
        return (cities ?? []).sort((c1, c2) => {
            const r1 = (c1?.region ?? '').toLocaleUpperCase();
            const r2 = (c2?.region ?? '').toLocaleUpperCase();
            const s1 = (c1?.subregion ?? '').toLocaleUpperCase();
            const s2 = (c2?.subregion ?? '').toLocaleUpperCase();
            const n1 = (c1?.name ?? '').toLocaleUpperCase();
            const n2 = (c2?.name ?? '').toLocaleUpperCase();
            const d1 = (c1?.default ?? false);
            const d2 = (c2?.default ?? false);

            if (d1 != d2) {
                if (d1) return -1;
                else if (d2) return 1;
            }

            let dd = GeographyApiService.compareStrings(n1, n2, terms);
            if (dd !== 0) return dd;

            dd = GeographyApiService.compareStrings(s1, s2, terms);
            if (dd !== 0) return dd;

            dd = GeographyApiService.compareStrings(r1, r2, terms);
            if (dd !== 0) return dd;

            return dd;
        });
    }

    private static compareStrings(v1: string, v2: string, terms: string[]) {

        if (terms?.length > 0) {
            const m1 = GeographyApiService.isFullMatch(v1, terms);
            const m2 = GeographyApiService.isFullMatch(v2, terms);
    
            if (m1 !== m2) {
                if (m1) return -1;
                else if (m2) return 1;
            }
        }
        
        return v1.localeCompare(v2);
    }

    private static isFullMatch(str: string, terms: string[]): boolean {
        str = (str ?? '').toLocaleUpperCase();
        for (let tt of terms) {
            if (str === tt.toLocaleUpperCase())
                return true;
        }
        return false;
    }

    private getAllCities(): Observable<City[]> {
        const uri = `${environment.apiUrl}/geography/cities/all`;
        return this.httpClient.get<City[]>(uri);
    }
}

@UntilDestroy()
@Injectable()
export class MeteoApiService {
    constructor(private readonly httpClient: HttpClient) {
    }

    public getStudioDownloadUrl(): Observable<string> {
        const uri = `${environment.apiUrl}/meteo/studioDownloadUrl`;
        return this.httpClient.get<string>(uri);
    }

    public getRange(region: string): Observable<CalendarRange> {
        const uri = `${environment.apiUrl}/meteo/range?region=${region}`;
        return this.httpClient.get<CalendarRange>(uri);
    }

    public getData(region: string, subregion: string, city: string, skip: number = 0, take: number = 0): Observable<MeteoData> {
        const uri = `${environment.apiUrl}/meteo/data?region=${region}&subregion=${subregion}&city=${city}&skip=${skip}&take=${take}`;
        return this.httpClient.get<MeteoData>(uri);
    }
}
