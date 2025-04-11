import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { ContentUnit } from 'src/app/models/models-swagger';
import { environment } from 'src/environments/environment';

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

    public uploadPlainContent(path: string, content: string): Observable<any> {
        const uri = `${environment.apiUrl}/Content/plain/${path}`;
        const headers = new HttpHeaders({ 'Content-Type': 'text/plain' });
        return this.httpClient.post(uri.toString(), content, { headers });
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
