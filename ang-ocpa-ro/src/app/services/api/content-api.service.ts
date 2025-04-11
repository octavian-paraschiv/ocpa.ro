import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { ContentUnit } from 'src/app/models/models-swagger';
import { environment } from 'src/environments/environment';
import * as CryptoJS from 'crypto-js';

@UntilDestroy()
@Injectable()
export class ContentApiService {
    constructor(private readonly httpClient: HttpClient) {
    }

    public renderContent(path: string): Observable<string> {
        const uri = `${environment.apiUrl}/Content/render/${path}`;
        return this.httpClient.get(uri, { 
            responseType: 'text',
            headers: { 
                'Cache-Control': 'no-cache',
                'X-RenderAsHtml': 'false',
            }
        });
    }

    public getContent(path: string): Observable<string> {
        const uri = `${environment.apiUrl}/Content/${path}`;
        return this.httpClient.get<string>(uri, { 
            headers: { 
                'Cache-Control': 'no-cache'
            }
        });
    }

    public uploadContent(path: string, content: ArrayBuffer, contentType: string): Observable<any> {
        const uri = `${environment.apiUrl}/Content/upload/${path}`;
        const formData = new FormData();
        const signature = CryptoJS.HmacSHA1(CryptoJS.lib.WordArray.create(content), path);
        const blob = new Blob([content], { type: contentType });

        formData.append("signature", signature.toString(CryptoJS.enc.Base64));
        formData.append("data", blob, path);

        return this.httpClient.post(uri, formData);
    }

    public listContent(path: string = undefined, 
        level: number = 0, 
        filter: string = undefined): Observable<ContentUnit> {

        if (!(path?.length > 0))
            path = '.';

        const uri = new URL(`${environment.apiUrl}/Content/list/${path}`);
        
        if (level > 0)
            uri.searchParams.append('level', `${level}`);
        if (filter?.length > 0)
            uri.searchParams.append('filter', filter);

        return this.httpClient.get<ContentUnit>(uri.toString());
    }
}
