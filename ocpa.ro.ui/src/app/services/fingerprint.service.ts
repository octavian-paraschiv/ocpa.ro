import { Injectable } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import FingerprintJS from '@fingerprintjs/fingerprintjs';
import { from, Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';

@UntilDestroy()
@Injectable()
export class FingerprintService {
    private _fingerprint: string = '';

    constructor() {
    }

    public get Fingerprint(): string {
        return this._fingerprint;
    }

    public init(): Observable<boolean> {
        console.debug('Calling FingerprintService.init...');
        return from(FingerprintJS.load()).pipe(
            untilDestroyed(this),
            switchMap(res => from(res.get())),
            map(res => {
                this._fingerprint = res?.visitorId;
                return true;
            }),
            catchError(err => {
                console.error(err);
                return of(false);
            }));
    }
}
