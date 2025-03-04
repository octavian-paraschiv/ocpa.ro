import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@UntilDestroy()
@Injectable()
export class WikiService {
    constructor(private readonly httpClient: HttpClient) {
    }

    public getWiki(location: string): Observable<string> {
        const uri = `${environment.apiUrl}/wiki/${location}`;
        return this.httpClient.get(uri, { 
            responseType: 'text',
            headers: { 
                'Cache-Control': 'no-cache',
                'X-HtmlFragment': 'true',
            }
        });
    }
}
