import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { ContentUnit } from 'src/app/models/swagger/content-management';
import { environment } from 'src/environments/environment';
import * as CryptoJS from 'crypto-js';

@UntilDestroy()
@Injectable()
export class ContentApiService {
    constructor(private readonly httpClient: HttpClient) {
    }

    public renderContent(path: string, renderTranslated: boolean): Observable<string> {
        const uri = `${environment.apiUrl}/Content/render/${path}`;
        let headers: { [key: string]: string } = {};

        headers['Cache-Control'] = 'no-cache';
        headers['X-RenderAsHtml'] = 'false';
        
        if (renderTranslated) {
            headers['X-Language'] = navigator.language ?? 'en';
        }

        return this.httpClient.get(uri, { responseType: 'text', headers });
    }

    public getContent(path: string): Observable<string> {
        const uri = `${environment.apiUrl}/Content/${path}`;
        return this.httpClient.get<string>(uri, { 
            headers: { 
                'Cache-Control': 'no-cache'
            }
        });
    }

    public createFolder(path: string): Observable<ContentUnit> {
        const uri = `${environment.apiUrl}/Content/folder/${path}`;
        return this.httpClient.post(uri, undefined);
    }

    public deleteContent(path: string): Observable<any> {
        const uri = `${environment.apiUrl}/Content/delete/${path}`;
        return this.httpClient.post(uri, undefined);
    }

    public uploadContent(path: string, content: ArrayBuffer, contentType: string): Observable<ContentUnit> {
        const uri = `${environment.apiUrl}/Content/upload/${path}`;
        const formData = new FormData();
        const signature = CryptoJS.HmacSHA1(CryptoJS.lib.WordArray.create(content), path);
        const blob = new Blob([content], { type: contentType });

        formData.append("signature", signature.toString(CryptoJS.enc.Base64));
        formData.append("data", blob, path);

        return this.httpClient.post(uri, formData);
    }

    public moveContent(path: string, newPath: string): Observable<ContentUnit> {
        const uri = `${environment.apiUrl}/Content/move/${path}?newPath=${newPath}`;
        return this.httpClient.post(uri, undefined);
    }

    public listContent(path: string, level: number, filter: string, markdownView: boolean): Observable<ContentUnit> {

        if (!(path?.length > 0))
            path = '.';

        const uri = new URL(`${environment.apiUrl}/Content/list/${path}`);
        
        if (level > 0)
            uri.searchParams.append('level', `${level}`);
        if (filter?.length > 0)
            uri.searchParams.append('filter', filter);
        if (!!markdownView)
            uri.searchParams.append('markdownView', `${markdownView}`);

        return this.httpClient.get<ContentUnit>(uri.toString());
    }
}
