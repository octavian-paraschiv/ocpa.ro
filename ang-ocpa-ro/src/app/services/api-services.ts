import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "src/environments/environment";
import { City, GridCoordinates } from "../models/geography";
import { CalendarRange, MeteoData } from "../models/meteo";
import { BuildInfo  } from "../models/protone";

@Injectable()
export class ProtoneApiService {
    constructor(private readonly httpClient: HttpClient) {
    }

    public getProtoneBuilds(type: string): Observable<BuildInfo[]> {
        const uri = `${environment.apiUrl}/protone?release=${type}`;
        return this.httpClient.get<BuildInfo[]>(uri);
    }
}

@Injectable()
export class GeographyApiService {
    constructor(private readonly httpClient: HttpClient) {
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
}

@Injectable()
export class MeteoApiService {
    constructor(private readonly httpClient: HttpClient) {
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
