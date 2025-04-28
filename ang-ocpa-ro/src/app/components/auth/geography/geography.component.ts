import { AfterViewInit, Component, ViewChild, inject } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { faEye, faSquarePlus, faSquarePen, faSquareMinus, faCheck } from '@fortawesome/free-solid-svg-icons';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { of } from 'rxjs';
import { filter, first, switchMap } from 'rxjs/operators';
import { CityDialogComponent } from 'src/app/components/auth/geography/city-dialog/city-dialog.component';
import { BaseAuthComponent } from 'src/app/components/base/BaseComponent';
import { MessageBoxComponent } from 'src/app/components/shared/message-box/message-box.component';
import { MessageBoxOptions } from 'src/app/models/models-local';
import { CityDetail } from 'src/app/models/models-swagger';
import { GeographyApiService } from 'src/app/services/api/geography-api.service';

@UntilDestroy()
@Component({
    selector: 'app-geography',
    templateUrl: './geography.component.html'
})
export class GeographyComponent extends BaseAuthComponent implements AfterViewInit {
    faEye = faEye;
    faAdd = faSquarePlus;
    faEdit = faSquarePen;
    faRemove = faSquareMinus;
    faCheck = faCheck;
    size = "grow-6";

    citiesColumns: string[] = [ 'city-add', 'city-edit', 'city-delete', 'city-name', 
        'city-region', 'city-subregion', 'city-latitude', 'city-longitude', 'city-default', 'city-filler' ];

    private readonly geographyService = inject(GeographyApiService);
    
    searchTerm = '';

    cities = new MatTableDataSource<CityDetail>([]);
    @ViewChild(MatPaginator) paginator: MatPaginator;

    ngAfterViewInit() {
       this.searchCities('');
    }

    searchCities(searchTerm: string) {
        this.searchTerm = searchTerm;
        this.cities.paginator = this.paginator;
        this.cities.data = (this.searchTerm?.length > 0) ?
            GeographyApiService.FilterCities(this.searchTerm, this.geographyService.cities) :
            this.geographyService.cities;
    }

    saveCity(city: CityDetail = undefined) {
        CityDialogComponent.showDialog(this.dialog, city).pipe(
            first(),
            untilDestroyed(this),
            switchMap(cd => cd?.id === -1 ? of(cd) : this.geographyService.saveCity(cd))                
        ).subscribe({
            next: (cd) => {
                if (cd) {
                    if (cd?.id > 0) {
                        this.geographyService
                            .init()
                            .pipe(untilDestroyed(this), filter(res => !!res))
                            .subscribe(() => this.searchCities(this.searchTerm));
                        this.popup.showSuccess('geography.success-save-city', { name: cd.name });
                    }
                } else {
                    this.popup.showError('users.error-save-city');
                }
            },
            error: err => this.popup.showError(err.toString(), { name: city.name })
        });
    }

    onDelete(city: CityDetail) {
        MessageBoxComponent.show(this.dialog, {
            title: this.translate.instant('title.confirm'),
            message: this.translate.instant('geography.delete-city', { name: city.name })
        } as MessageBoxOptions)
        .pipe(untilDestroyed(this))
        .subscribe(res => {
            if (res) {
                this.geographyService.deleteCity(city.id)
                .pipe(untilDestroyed(this))
                .subscribe({
                    next: () => {
                        this.geographyService
                            .init()
                            .pipe(untilDestroyed(this), filter(res => !!res))
                            .subscribe(() => this.searchCities(this.searchTerm));
                        this.popup.showSuccess('geography.success-delete-city', { name: city.name });
                    },
                    error: err => this.popup.showError(err.toString(), { name: city.name, 
                        subregion: `${city.regionName} > ${city.subregion}` })
                });
            }
        });
    }
}