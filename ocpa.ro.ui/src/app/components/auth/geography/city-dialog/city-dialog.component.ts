import { Component, OnInit, AfterViewInit } from '@angular/core';
import { UntypedFormGroup, UntypedFormBuilder, Validators } from '@angular/forms';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Observable, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { CityDetail, RegionDetail } from 'src/app/models/models-swagger';
import { GeographyApiService } from 'src/app/services/api/geography-api.service';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';

@UntilDestroy()
@Component({
  selector: 'city-dialog.component',
  templateUrl: './city-dialog.component.html'
})
export class CityDialogComponent implements OnInit, AfterViewInit {
  cityForm: UntypedFormGroup;
  editMode = false;

  selectedRegion: RegionDetail = { minLat: 0, maxLat: 0, minLon: 0, maxLon: 0 };
  selectedSubregion: string = undefined;
  regions: RegionDetail[] = [];

  center: google.maps.LatLngLiteral = { lat: 24, lng: 12 };
  zoom = 12;

  public city: CityDetail;
  public result$ = new Subject<CityDetail>();

  constructor(
    private geographyService: GeographyApiService,
    private formBuilder: UntypedFormBuilder,
    public bsModalRef: BsModalRef
  ) {}

  ngAfterViewInit(): void {
    setTimeout(() => {
      const map = document.getElementById("map");
      const x = map?.querySelector("div > div:nth-child(2) > table > tr > td:nth-child(2) > button");
      (x as HTMLButtonElement)?.click();
    }, 500);
  }

  ngOnInit() {
    this.editMode = this.city?.id > 0;

    if (!this.city) {
      this.city = {
        id: 0,
        isDefault: false,
        lat: 0,
        lon: 0
      } as CityDetail;
    }

    this.cityForm = this.formBuilder.group({
      name: [this.city.name, [Validators.required, Validators.maxLength(24)]],
      region: [this.city.regionName, [Validators.required, Validators.maxLength(24)]],
      subregion: [this.city.subregion, [Validators.required, Validators.maxLength(24)]],
      latitude: [this.city.lat, [Validators.required]],
      longitude: [this.city.lon, [Validators.required]],
    });

    this.geographyService.getAllRegions()
      .pipe(untilDestroyed(this))
      .subscribe(regions => {
        this.regions = regions;
        if (this.editMode) {
          this.selectedRegion =
            this.regions.find(r => r.id === this.city.regionId) ?? this.regions[0];

          this.selectedSubregion =
            this.selectedRegion.subregions.find(sr => sr === this.city.subregion) ??
            (this.selectedRegion?.code === 'EU' ? 'Poland' : 'Brasov');

          this.zoom = this.selectedRegion?.code === 'EU' ? 10 : 12;
        } else {
          this.selectedRegion = this.regions[0];
          this.selectedSubregion = this.selectedRegion?.code === 'EU' ? 'Poland' : 'Brasov';
          this.city.lat = 0.5 * this.selectedRegion.minLat + 0.5 * this.selectedRegion.maxLat;
          this.city.lon = 0.5 * this.selectedRegion.minLon + 0.5 * this.selectedRegion.maxLon;
          this.zoom = this.selectedRegion?.code === 'EU' ? 4 : 6;
        }

        this.center = { lat: this.city.lat, lng: this.city.lon };
        this.attachLatLonValidators();
      });
  }

  get f() { return this.cityForm?.controls; }

  get title(): string {
    return this.editMode ? 'city-dialog.edit' : 'city-dialog.create';
  }

  onRegionChanged() {
    this.selectedSubregion = this.selectedRegion.subregions[0];
    this.attachLatLonValidators();
  }

  onClickMap(event: google.maps.MapMouseEvent) {
    this.f.latitude.setValue(parseFloat(event.latLng.lat().toFixed(2)));
    this.f.longitude.setValue(parseFloat(event.latLng.lng().toFixed(2)));
    event.stop();
  }

  updateZoom() {
    if (this.editMode) {
      this.zoom = this.selectedRegion?.code === 'EU' ? 10 : 12;
    } else {
      this.selectedSubregion = this.selectedRegion?.code === 'EU' ? 'Poland' : 'Brasov';
      this.city.lat = 0.5 * this.selectedRegion.minLat + 0.5 * this.selectedRegion.maxLat;
      this.city.lon = 0.5 * this.selectedRegion.minLon + 0.5 * this.selectedRegion.maxLon;
      this.zoom = this.selectedRegion?.code === 'EU' ? 4 : 6;
    }

    this.center = { lat: this.city.lat, lng: this.city.lon };
  }

  attachLatLonValidators() {
    this.f.latitude.clearValidators();
    this.f.longitude.clearValidators();
    this.f.latitude.addValidators([
      Validators.required,
      Validators.min(this.selectedRegion.minLat),
      Validators.max(this.selectedRegion.maxLat)
    ]);
    this.f.longitude.addValidators([
      Validators.required,
      Validators.min(this.selectedRegion.minLon),
      Validators.max(this.selectedRegion.maxLon)
    ]);

    this.updateZoom();
  }

  onCancel(): void {
    this.result$.next({ id: -1 });
    this.bsModalRef.hide();
  }

  onOk(): void {
    this.cityForm.updateValueAndValidity();
    if (this.cityForm.invalid) {
      return;
    }

    this.result$.next({
      id: this.city.id,
      name: this.f.name.value,
      regionName: this.selectedRegion.name,
      subregion: this.selectedSubregion,
      lat: this.f.latitude.value,
      lon: this.f.longitude.value,
      isDefault: this.editMode && this.city.isDefault
    } as CityDetail);

    this.bsModalRef.hide();
  }

  static showDialog(modalService: BsModalService, city?: CityDetail): Observable<CityDetail> {
    const bsModalRef = modalService.show(CityDialogComponent, {
      initialState: { city },
      class: 'bs-modal-city'
    });

    return (bsModalRef.content as CityDialogComponent).result$.asObservable().pipe(
      map(result => result ?? { id: -1 })
    );
  }
}