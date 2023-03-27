import { Injectable } from "@angular/core";

@Injectable()
export class Helper {
    private feelsLikeMap: { [id: string]: { color: string, tip: string}  } = {};
    private weatherTypeMap: { [id: string]:string } = {};

    constructor() {
      this.feelsLikeMap['normal'] =       { color:'white',      tip: 'Seasonable' };
      this.feelsLikeMap['warmer'] =       { color:'#ffffcc',    tip: 'Warmer' };
      this.feelsLikeMap['much_warmer'] =  { color:'#ffff66',    tip: 'Much warmer' };
      this.feelsLikeMap['hot'] =          { color:'#ffcc99',    tip: 'Hot' };      
      this.feelsLikeMap['colder'] =       { color:'#e6f7ff',    tip: 'Colder' };
      this.feelsLikeMap['much_colder'] =  { color:'#e6efff',    tip: 'Very cold' };
      this.feelsLikeMap['frost'] =        { color:'#ccdfff',    tip: 'Frost' };

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