import { Injectable, inject } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';
import { CookieService } from 'ngx-cookie-service';
import { UserSessionInformation } from 'src/app/models/models-local';

@UntilDestroy()
@Injectable()
export class SessionInformationService {
    private readonly userSessionKey = 'ocpa_ro_user_session';
    private readonly defaultExpiration = 1 / 48; // half an hour as days
    private readonly cookieService = inject(CookieService);

    public set<T>(data: T, key: string, expiration: number) {
        try {
            this.cookieService.set(key, JSON.stringify(data), expiration);
        } catch {
        }
    }

    public setUserSessionInformation(data: UserSessionInformation, expiration?: number) {
        expiration = expiration ?? this.defaultExpiration;
        this.set<UserSessionInformation>(data, this.userSessionKey, expiration);
    }

    public get<T>(key: string): T {
        try {
            const json = this.cookieService.get(key);
            if (json?.length > 0) 
                return JSON.parse(json) as T;
        } catch {
        }

        return undefined;
    }

    public getUserSessionInformation() {
        return this.get<UserSessionInformation>(this.userSessionKey);
    }

    public delete(key?: string) {
        try {
            this.cookieService.delete(key);
        } catch {
        }
    }

    public clearUserSessionInformation() {
        return this.delete(this.userSessionKey);
    }
}