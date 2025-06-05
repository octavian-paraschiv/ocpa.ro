import { formatDate } from '@angular/common';
import { inject } from '@angular/core';
import { ActivatedRouteSnapshot } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';

export class Helper {
    public static translateTitle(route: ActivatedRouteSnapshot, translate: TranslateService): string {
        let rawTitle = 'unavailable';
        const url = route.url;
        const path = route.routeConfig?.path;
        if (url?.length > 0 && path !== '**')
          rawTitle = url.map(s => s.path).join('.');
      
        return translate.instant(`title.${rawTitle}`);
      };

    public static get today(): string {
        return this.isoDate(new Date());
    }

    public static isoDate(date: string | Date, includeTime: boolean = false) {
        return includeTime ?
            formatDate(date, 'yyyy-MM-dd HH:mm:ss', 'en-US') :
            formatDate(date, 'yyyy-MM-dd', 'en-US');
    }

    public static addDays(date: string | Date, days: number): Date {
        const result = new Date(date);
        result.setDate(result.getDate() + days);
        return result;
    }

    public static displayMode(): string {
       return (window.innerWidth < 500) ? "m1" : 
        (window.innerWidth < 640) ? "m2" : 
        (window.innerWidth < 1080) ? "m3" : 
        "m4";
    }

    public static isMobile(): boolean {
        return (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|Mobile|mobile|CriOS/i)
                .test(navigator?.userAgent);
    }

    public static tryDecodeAsText(base64: string): string {
        try {
            // Decode the Base64 string
            const decodedBytes = window.atob(base64);
            if (decodedBytes?.length > 0 && decodedBytes.length < 1024 * 1024) {
                // Convert the decoded bytes to a string
                const decodedString = new TextDecoder().decode(new Uint8Array(decodedBytes.split('').map(char => char.charCodeAt(0))));

                // Check if the string contains valid UTF-8 characters
                if (/^[\x09-\x0D\x20-\x7F\x80-\u07FF]*$/.test(decodedString)) {
                    return decodedString;
                }
            }
        } catch {
            // If decoding fails, base64 was likely not a valid base-64 string
        }

        return undefined;
    }

    public static tryDecodeAsImage(base64: string): string {
        const header = Helper.getHeaderChars(base64);
        if (header?.length > 0) {
            if (header.startsWith('FF D8 FF DB') ||
                header.startsWith('FF D8 FF E0') ||
                header.startsWith('FF D8 FF E1') ||
                header.startsWith('FF D8 FF EE') ||
                header.startsWith('FF 4F FF 51') ||
                header.startsWith('00 00 00 0C 6A 50 20 20 0D')) {
                return `data:image/jpeg;base64,${base64}`;
      
            } else if (header.startsWith('47 49 46 38 37 61') ||
                header.startsWith('47 49 46 38 39 61')) {
                return `data:image/gif;base64,${base64}`;

            } else if (header.startsWith('89 50 4E 47 0D 0A 1A 0A')) {
                return `data:image/png;base64,${base64}`;

            } else if (header.startsWith('42 4D')) {
                return `data:image/bmp;base64,${base64}`;
            }
        }

      return undefined;
    }

    public static getHeaderChars(base64: string): string {
        try {
            // Decode the Base64 string
            const header = window.atob(base64.substring(0, Math.min(base64.length - 1, 20)) ?? '');
            return header
            .split('')
            .map(char => char.charCodeAt(0).toString(16).toUpperCase().padStart(2, '0'))
            .join(' ');

        } catch {
            // If decoding fails, base64 was likely not a valid base-64 string
        }

        return undefined;
    }
}