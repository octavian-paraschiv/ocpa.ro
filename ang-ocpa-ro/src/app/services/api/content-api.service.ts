import { HttpClient } from '@angular/common/http';
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
