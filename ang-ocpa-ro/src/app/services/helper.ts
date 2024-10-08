import { formatDate } from '@angular/common';
import { Injectable } from "@angular/core";

@Injectable()
export class Helper {
    private feelsLikeMap: { [id: string]: string } = {};
    private weatherTypeMap: { [id: string]: string } = {};

    constructor() {
      this.feelsLikeMap['normal'] = 'Seasonable';
      this.feelsLikeMap['warmer'] = 'Warmer';
      this.feelsLikeMap['much_warmer'] = 'Very warm';
      this.feelsLikeMap['hot'] = 'Hot';      
      this.feelsLikeMap['colder'] = 'Colder';
      this.feelsLikeMap['much_colder'] = 'Very cold';
      this.feelsLikeMap['frost'] = 'Frost';

      this.weatherTypeMap['00'] = 'Sunny';
      
      this.weatherTypeMap['01_wind'] = 'Gentle wind';
      this.weatherTypeMap['01_rain'] = 'Light rain';
      this.weatherTypeMap['01_inst'] = 'Light rain & thunder';
      this.weatherTypeMap['01_snow'] = 'Light snow';
      this.weatherTypeMap['01_mix'] =  'Light wintry mix';
      this.weatherTypeMap['01_fog'] =  'Morning Fog';
      this.weatherTypeMap['01_ice'] =  'Icy drizzle';
      
      this.weatherTypeMap['02_rain'] = 'Rain';
      this.weatherTypeMap['02_inst'] = 'T-Storm';
      this.weatherTypeMap['02_snow'] = 'Snow';
      this.weatherTypeMap['02_mix'] =  'Wintry mix';
      this.weatherTypeMap['02_fog'] =  'Fog';
      this.weatherTypeMap['02_ice'] =  'Icy rain';
      this.weatherTypeMap['02_wind'] = 'Windy';

      this.weatherTypeMap['03_rain'] = 'Intense rain';
      this.weatherTypeMap['03_inst'] = 'Intense T-Storm';
      this.weatherTypeMap['03_snow'] = 'Intense snow';
      this.weatherTypeMap['03_mix'] =  'Intense wintry mix';
      this.weatherTypeMap['03_fog'] =  'Thick fog';
      this.weatherTypeMap['03_ice'] =  'Strong icy rain';
      this.weatherTypeMap['03_wind'] = 'Strong wind';

      this.weatherTypeMap['04_rain'] = 'Heavy rain';
      this.weatherTypeMap['04_inst'] = 'Severe T-Storm';
      this.weatherTypeMap['04_snow'] = 'Heavy snow';
      this.weatherTypeMap['04_mix'] =  'Heavy wintry mix';
      this.weatherTypeMap['04_fog'] =  'Very thick fog';
      this.weatherTypeMap['04_ice'] =  'Heavy icy rain';
      this.weatherTypeMap['04_wind'] = 'Heavy Wind';
    }

    public feelsLikeTip(desc: string): string {
        return this.feelsLikeMap[desc];
    }

    public weatherType(desc: string): string {
        return this.weatherTypeMap[desc];
    }

    public get today(): string {
        return this.isoDate(new Date());
    }

    public isoDate(date: string | Date, includeTime: boolean = false) {
        return includeTime ?
            formatDate(date, 'yyyy-MM-dd HH:mm:ss', 'en-US') :
            formatDate(date, 'yyyy-MM-dd', 'en-US');
    }

    public addDays(date: string | Date, days: number): Date {
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