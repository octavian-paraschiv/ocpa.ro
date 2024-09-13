import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, of } from "rxjs";
import { environment } from "src/environments/environment";
import { UntilDestroy, untilDestroyed } from "@ngneat/until-destroy";
import { catchError, map } from "rxjs/operators";
import { BuildInfo, City, GridCoordinates, MeteoData, ContentUnit, MeteoDbInfo } from 'src/app/models/models-swagger';
import * as pako from 'pako';
import * as CryptoJS from 'crypto-js';

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
        const uri = `${environment.apiUrl}/Meteo/studio-download-url`;
        return this.httpClient.get<string>(uri);
    }

    public getDatabases(): Observable<MeteoDbInfo[]> {
        const uri = `${environment.apiUrl}/Meteo/databases/all`;
        return this.httpClient.get<MeteoDbInfo[]>(uri);
    }

    public getData(dbi: number, region: string, subregion: string, city: string, skip: number = 0, take: number = 0): Observable<MeteoData> {
        const endpointRoot = `${environment.apiUrl}/Meteo/data`;
        const queryString = `region=${region}&subregion=${subregion}&city=${city}&skip=${skip}&take=${take}`;
        const uri = (dbi >= 0) ? 
            `${endpointRoot}/preview/${dbi}?${queryString}` :
            `${endpointRoot}?${queryString}`;

        return this.httpClient.get<MeteoData>(uri);
    }

    public promote(dbi: number): Observable<any> {
        const uri = `${environment.apiUrl}/Meteo/database/preview/promote/${dbi}`;
        return this.httpClient.post(uri, {});
    }

    public upload(dbi: number, data: ArrayBuffer): Observable<any> {
        const uri = `${environment.apiUrl}/Meteo/database/preview/${dbi}`;
        const formData = new FormData();
        const fileName = `Preview${dbi}.db3`;
        const compressed = pako.gzip(new Uint8Array(data));
        const signature = CryptoJS.HmacSHA1(CryptoJS.lib.WordArray.create(compressed), fileName);
        const blob = new Blob([compressed], { type: 'application/gzip' });

        formData.append("signature", signature.toString(CryptoJS.enc.Base64));
        formData.append("data", blob, fileName);

        return this.httpClient.post(uri, formData);
    }
}

@UntilDestroy()
@Injectable()
export class ContentApiService {
    constructor(private readonly httpClient: HttpClient) {
    }

    public getContent(path: string = undefined, 
        level: number = 0, 
        filter: string = undefined): Observable<ContentUnit> {

        if (!(path?.length > 0))
            path = '.';

        const uri = new URL(`${environment.apiUrl}/Content/${path}`);
        
        if (level > 0)
            uri.searchParams.append('level', `${level}`);
        if (filter?.length > 0)
            uri.searchParams.append('filter', filter);

        return this.httpClient.get<ContentUnit>(uri.toString());
    }
}

