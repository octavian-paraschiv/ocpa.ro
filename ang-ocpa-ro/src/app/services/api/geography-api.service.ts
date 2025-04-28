import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { CityDetail, GridCoordinates, RegionDetail } from 'src/app/models/models-swagger';
import { environment } from 'src/environments/environment';

@UntilDestroy()
@Injectable()
export class GeographyApiService {

    private _cities: CityDetail[] = [];

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
                    console.error(err.toString());
                    this._cities = [];
                    return of(false);
                })
            )
            
    }

    public getAllRegions(): Observable<RegionDetail[]> {
        const uri = `${environment.apiUrl}/geography/regions/all`;
        return this.httpClient.get<RegionDetail[]>(uri);
    }

    public getRegionNames(): Observable<string[]> {
        const uri = `${environment.apiUrl}/geography/regions/names`;
        return this.httpClient.get<string[]>(uri);
    }

    public getSubregionNames(region: string): Observable<string[]> {
        const uri = `${environment.apiUrl}/geography/subregions/names?region=${region}`;
        return this.httpClient.get<string[]>(uri);
    }

    public getCityNames(region: string, subregion: string): Observable<string[]> {
        const uri = `${environment.apiUrl}/geography/cities/names?region=${region}&subregion=${subregion}`;
        return this.httpClient.get<string[]>(uri);
    }

    public getCityInfo(region: string, subregion: string, city: string): Observable<CityDetail> {
        const uri = `${environment.apiUrl}/geography/city?region=${region}&subregion=${subregion}&city=${city}`;
        return this.httpClient.get<CityDetail>(uri);
    }

    public getGridCoordinates(region: string, subregion: string, city: string): Observable<GridCoordinates> {
        const uri = `${environment.apiUrl}/geography/grid?region=${region}&subregion=${subregion}&city=${city}`;
        return this.httpClient.get<GridCoordinates>(uri);
    }

    public get cities(): CityDetail[] {
        return GeographyApiService.SortCities([], this._cities);
    }

    public saveCity(city: CityDetail): Observable<CityDetail> {
        return this.httpClient.post<CityDetail>(`${environment.apiUrl}/geography/city/save`, city);
    }

    public deleteCity(id: number): Observable<Object> {
        return this.httpClient.post(`${environment.apiUrl}/geography/city/delete/${id}`, undefined);
    }

    public static FilterCities(searchTerm: string, cities: CityDetail[]): CityDetail[] {
        const allCities = cities ?? [];
        let filteredCities: CityDetail[] = [];
        if (allCities.length > 5) {
            if (searchTerm?.length > 0) {
                const terms = searchTerm.toLocaleUpperCase().split(' ').filter(t => t?.length > 0);
                filteredCities = GeographyApiService.SortCities(terms, allCities.filter(city => {
                let match = true;
                // All search terms must match
                for (let tt of terms) {
                    let termMatch = false;
                    termMatch ||= (city?.name ?? '').toLocaleUpperCase().includes(tt);
                    termMatch ||= (city?.subregion ?? '').toLocaleUpperCase().startsWith(tt);
                    termMatch ||= (city?.regionName ?? '').toLocaleUpperCase().startsWith(tt);
                    match &&= termMatch;
                    if (!match) break;
                }
                return match;
                }));
        
            } else {
                // no filter specified => no filtering needed
                filteredCities = allCities;
            }
        } else {
            // Less than 5 cities to show => no filtering needed
            filteredCities = allCities;
        }
        
        return filteredCities;
    }

    public static SortCities(terms: string[], cities: CityDetail[]): CityDetail[] {
        return (cities ?? []).sort((c1, c2) => {
            const r1 = (c1?.regionName ?? '').toLocaleUpperCase();
            const r2 = (c2?.regionName ?? '').toLocaleUpperCase();
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

    private getAllCities(): Observable<CityDetail[]> {
        const uri = `${environment.apiUrl}/geography/cities/all`;
        return this.httpClient.get<CityDetail[]>(uri);
    }
}