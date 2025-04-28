import { Component, OnInit, Inject } from '@angular/core';
import { UntypedFormGroup, UntypedFormBuilder, Validators, AbstractControl, ValidatorFn } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { CityDetail, RegionDetail } from 'src/app/models/models-swagger';
import { GeographyApiService } from 'src/app/services/api/geography-api.service';

@UntilDestroy()
@Component({
    selector: 'city-dialog.component',
    templateUrl: './city-dialog.component.html'
})
export class CityDialogComponent implements OnInit {
    cityForm: UntypedFormGroup;
    hide = true;
    editMode = false;

    selectedRegion: RegionDetail = { minLat: 0, maxLat: 0, minLon: 0, maxLon: 0 };
    selectedSubregion: string = undefined;
    regions: RegionDetail[] = [];

    get subregions(): string[] {
        const regionName = this.f?.region?.value;
        return this.regions.find(r => r.name === regionName)?.subregions ?? [];
    }

    constructor(
        private geographyService: GeographyApiService,
        private formBuilder: UntypedFormBuilder,
        public dialogRef: MatDialogRef<CityDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public city: CityDetail
    ) {
    }

    ngOnInit() {
        this.editMode = this.city?.id > 0;

        if (!this.city)
            this.city = {
                id: 0,
                default: false,
                lat: 0,
                lon: 0
            } as CityDetail;

        this.cityForm = this.formBuilder.group({
            name: [ this.city.name, [ Validators.required, Validators.maxLength(24) ] ],
            region: [ this.city.regionName, [ Validators.required, Validators.maxLength(24) ] ],
            subregion: [ this.city.subregion, [ Validators.required, Validators.maxLength(24) ] ],
            latitude: [ this.city.lat, [ Validators.required ] ],
            longitude: [ this.city.lon, [ Validators.required ] ],            
        });

        this.geographyService.getAllRegions()
            .pipe(untilDestroyed(this))
            .subscribe(regions => {
                this.regions = regions;
                if (this.editMode) {
                    this.selectedRegion = 
                        this.regions.find(r => r.id === this.city.regionId) ?? 
                        this.regions[0];

                    this.selectedSubregion = 
                        this.selectedRegion.subregions.find(sr => sr === this.city.subregion) ??
                        this.selectedRegion.subregions[0];

                } else {
                    this.selectedRegion = this.regions[0];
                    this.selectedSubregion = this.selectedRegion.subregions[0];
                    this.city.lat = this.selectedRegion.minLat;
                    this.city.lon = this.selectedRegion.minLon;
                }

                this.attachLatLonValidators();
            });
    }

    // convenience getter for easy access to form fields
    get f() { return this.cityForm?.controls; }

    get title(): string {
        return (this.editMode) ? 
            'city-dialog.edit' :
            'city-dialog.create';
    }

    onRegionChanged() {
        this.selectedSubregion = this.selectedRegion.subregions[0];
    }

    attachLatLonValidators() {
        this.f.latitude.clearValidators();
        this.f.longitude.clearValidators();
        this.f.latitude.addValidators([ Validators.required, 
            Validators.min(this.selectedRegion.minLat), Validators.max(this.selectedRegion.maxLat) ]);
        this.f.longitude.addValidators([ Validators.required,
            Validators.min(this.selectedRegion.minLon), Validators.max(this.selectedRegion.maxLon) ]);
    }

    onCancel(): void {
        this.dialogRef.close({id: -1});
    }

    onOk(): void {
        // stop here if form is invalid
        this.cityForm.updateValueAndValidity();
        if (this.cityForm.invalid) {
            return;
        }

        this.dialogRef.close({
            id: this.city.id,
            name: this.f.name.value,
            regionName: this.selectedRegion.name,
            subregion: this.selectedSubregion,
            lat: this.f.latitude.value,
            lon: this.f.longitude.value,
            default: this.editMode && this.city.default
        } as CityDetail);
    }

    static showDialog(dialog: MatDialog, city: CityDetail = undefined): Observable<CityDetail> {
        const dialogRef = dialog?.open(CityDialogComponent, { data: city, height: 'auto' });
        return dialogRef.afterClosed().pipe(map(result => (result ?? {id: -1}) as CityDetail));
    }
}
