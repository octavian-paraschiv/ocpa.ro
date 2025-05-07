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

    public static getHeaderChars(s: string): string {
        const header = (s?.substring(0, Math.min(s.length - 1, 10)) ?? '');
        return header
            .split('')
            .map(char => char.charCodeAt(0).toString(16).toUpperCase().padStart(2, '0'))
            .join(' ');
    }
}