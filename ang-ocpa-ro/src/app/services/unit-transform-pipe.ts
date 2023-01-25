import { Pipe, PipeTransform } from "@angular/core";
import { Unit } from "../models/meteo";

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

