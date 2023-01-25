import { Injectable } from "@angular/core";

@Injectable()
export class Helper {
    private feelsLikeMap: { [id: string]: { color: string, tip: string}  } = {};
    private weatherTypeMap: { [id: string]:string } = {};

    constructor() {
      this.feelsLikeMap['normal'] =       { color:'white',      tip: 'Normal temperature' };
      this.feelsLikeMap['warmer'] =       { color:'#ffffcc',    tip: 'Warmer than normal' };
      this.feelsLikeMap['much_warmer'] =  { color:'#ffff66',    tip: 'Much warmer than normal' };
      this.feelsLikeMap['hot'] =          { color:'#ffcc99',    tip: 'Heat wave' };      
      this.feelsLikeMap['colder'] =       { color:'#e6f7ff',    tip: 'Colder than normal' };
      this.feelsLikeMap['much_colder'] =  { color:'#e6efff',    tip: 'Much colder than normal' };
      this.feelsLikeMap['frost'] =        { color:'#ccdfff',    tip: 'Freezing cold' };

      this.weatherTypeMap['00'] = 'Sunny';
      
      this.weatherTypeMap['01_wind'] = 'Gentle breeze';
      this.weatherTypeMap['01_rain'] = 'Light rain';
      this.weatherTypeMap['01_inst'] = 'Light rain & thunder';
      this.weatherTypeMap['01_snow'] = 'Light snow';
      this.weatherTypeMap['01_mix'] =  'Light rain/snow mix';
      this.weatherTypeMap['01_fog'] =  'Some morning Fog';
      this.weatherTypeMap['01_ice'] =  'Icy drizzle';
      
      this.weatherTypeMap['02_rain'] = 'Rain';
      this.weatherTypeMap['02_inst'] = 'Stormy';
      this.weatherTypeMap['02_snow'] = 'Snow';
      this.weatherTypeMap['02_mix'] =  'Rain/snow mix';
      this.weatherTypeMap['02_fog'] =  'Fog';
      this.weatherTypeMap['02_ice'] =  'Icy rain';
      this.weatherTypeMap['02_wind'] = 'Windy';

      this.weatherTypeMap['03_rain'] = 'Intense rain';
      this.weatherTypeMap['03_inst'] = 'Thunderstorm';
      this.weatherTypeMap['03_snow'] = 'Intense snow';
      this.weatherTypeMap['03_mix'] =  'Intense rain/snow mix';
      this.weatherTypeMap['03_fog'] =  'Persistent fog';
      this.weatherTypeMap['03_ice'] =  'Strong icy rain';
      this.weatherTypeMap['03_wind'] = 'Strong wind';

      this.weatherTypeMap['04_rain'] = 'Heavy rain';
      this.weatherTypeMap['04_inst'] = 'Severe thunderstorm';
      this.weatherTypeMap['04_snow'] = 'Heavy snow';
      this.weatherTypeMap['04_mix'] =  'Heavy rain/snow mix';
      this.weatherTypeMap['04_fog'] =  'Extreme dense fog';
      this.weatherTypeMap['04_ice'] =  'Heavy icy rain';
      this.weatherTypeMap['04_wind'] = 'Heavy Wind';
    }

    public feelsLikeColor(desc: string): string {
        return this.feelsLikeMap[desc].color;
    }

    public feelsLikeTip(desc: string): string {
        return this.feelsLikeMap[desc].tip;
    }

    public weatherType(desc: string): string {
        return this.weatherTypeMap[desc];
    }
}