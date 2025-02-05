import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { BuildInfo } from 'src/app/models/models-swagger';
import { environment } from 'src/environments/environment';

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