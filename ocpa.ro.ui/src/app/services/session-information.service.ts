import { Injectable, inject } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';
import * as SecureLS from 'secure-ls';
import { UserSessionInformation } from 'src/app/models/local/access-management';

@UntilDestroy()
@Injectable()
export class SessionInformationService {
    private readonly userSessionKey = 'ocpa_ro_user_session';
    private readonly storage = new SecureLS({ encodingType: 'aes', isCompression: true });

    public set<T>(data: T, key: string) {
        try {
            this.storage.set(key, JSON.stringify(data));
        } catch {
        }
    }

    public setUserSessionInformation(data: UserSessionInformation) {
        this.set<UserSessionInformation>(data, this.userSessionKey);
    }

    public get<T>(key: string): T {
        try {
            const json = this.storage.get(key) as string;
            if (json?.length > 0) 
                return JSON.parse(json) as T;
        } catch {
        }

        return undefined;
    }

    public getUserSessionInformation() {
        return this.get<UserSessionInformation>(this.userSessionKey);
    }

    public delete(key: string) {
        try {
            this.storage.remove(key);
        } catch {
        }
    }

    public clearUserSessionInformation() {
        return this.delete(this.userSessionKey);
    }
}