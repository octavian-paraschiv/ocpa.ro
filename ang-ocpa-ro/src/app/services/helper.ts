import { formatDate } from '@angular/common';

export class Helper {

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

    public static isMobile(): boolean {
        return ((window?.innerWidth ?? 0) < 1080) ||
            (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|Mobile|mobile|CriOS/i)
                .test(navigator?.userAgent);
    }
}