import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { MeteoDbInfo, MeteoData } from 'src/app/models/models-swagger';
import { environment } from 'src/environments/environment';
import * as pako from 'pako';
import * as CryptoJS from 'crypto-js';

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
