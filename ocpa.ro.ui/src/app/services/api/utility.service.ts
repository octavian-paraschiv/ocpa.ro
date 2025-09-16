import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@UntilDestroy()
@Injectable()
export class UtilityService {
    constructor(private readonly httpClient: HttpClient) {
    }

    public getBackendVersion(): Observable<string> {
        const uri = `${environment.apiUrl}/utility/backend-version`;
        return this.httpClient.get(uri, { 
            responseType: 'text',
            headers: { 
                'Cache-Control': 'no-cache',
                'X-HtmlFragment': 'true',
            }
        });
    }
}
