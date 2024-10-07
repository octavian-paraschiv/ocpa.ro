import { Pipe, PipeTransform } from "@angular/core";
import { Unit } from 'src/app/models/models-local';
import { City, MeteoDailyData } from 'src/app/models/models-swagger';
import { Iso3166HelperService } from "./iso3166-helper.service";

@Pipe({ name: 'temp' })
export class TempPipe implements PipeTransform {
    transform(value: number, unit: Unit) {
        if (unit === Unit.Imperial) {
            return `${Math.round(9 * value / 5 + 32)}ºF`
        }
        return `${Math.round(value)}ºC`;
    }
}

@Pipe({ name: 'speed' })
export class SpeedPipe implements PipeTransform {
    transform(value: number, unit: Unit) {
        if (unit === Unit.Imperial) {
            return `${Math.round(value / 1.609)} mph`;
        }
        return `${Math.round(value)} kmh`;
    }
}

@Pipe({ name: 'distance' })
export class DistancePipe implements PipeTransform {
    transform(value: number, unit: Unit) {
        if (unit === Unit.Imperial) {
            return `${Math.round(value / 2.54)} in`;
        }
        return `${Math.round(value)} cm`;
    }
}

@Pipe({ name: 'volume' })
export class VolumePipe implements PipeTransform {
    transform(value: number, unit: Unit) {
        if (unit === Unit.Imperial) {
            return `${Math.round(value / 2.54)} in`;
        }
        return `${Math.round(value)} l/mp`;
    }
}

@Pipe({ name: 'pressure' })
export class PressurePipe implements PipeTransform {
    transform(value: number, unit: Unit) {
        if (unit === Unit.Imperial) {
            return `${Math.round(value / 33.863)} in`;
        }
        return `${Math.round(value)} mbar`;
    }
}

@Pipe({ name: 'countryCode' })
export class CountryCodePipe implements PipeTransform {
    constructor(private readonly isoService: Iso3166HelperService) {
    }

    transform(city: City) {
        let country = this.isoService.getByCountryName(city.subregion);
        if (!country)
            country = this.isoService.getByCountryName(city.region);
        
        return country?.IsoAlpha2?.toUpperCase() ?? 'XX';
    }
}

@Pipe({ name: 'calendar' })
export class CalendarPipe implements PipeTransform {
    transform(value: MeteoDailyData) {
        return `${value.date}`;
    }
}
