import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { RegisteredDevice } from 'src/app/models/swagger/access-management';

@Injectable({ providedIn: 'root' })
export class RegisteredDeviceService {

    constructor(private http: HttpClient) { }

    getAllRegisteredDevices(): Observable<RegisteredDevice[]> {
        return this.http.get<RegisteredDevice[]>(`${environment.apiUrl}/registered-devices/all`);
    }

    getRegisteredDevice(deviceId: string): Observable<RegisteredDevice> {
        return this.http.get<RegisteredDevice>(`${environment.apiUrl}/registered-devices/${deviceId}`);
    }

    deleteRegisteredDevice(deviceId: string): Observable<Object> {
        return this.http.post(`${environment.apiUrl}/registered-devices/delete/${deviceId}`, undefined);
    }
}