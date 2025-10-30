import { Component, inject, OnInit } from '@angular/core';
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
export class GeographyComponent extends BaseAuthComponent implements OnInit {
  faEye = faEye;
  faAdd = faSquarePlus;
  faEdit = faSquarePen;
  faRemove = faSquareMinus;
  faCheck = faCheck;
  size = 'grow-9';

  citiesColumns: string[] = [
    'city-add', 'city-edit', 'city-delete', 'city-name',
    'city-region', 'city-subregion', 'city-latitude',
    'city-longitude', 'city-default'
  ];

  private readonly geographyService = inject(GeographyApiService);

  searchTerm = '';
  cities: CityDetail[] = [];
  displayedCities: CityDetail[] = [];

  currentPage = 1;
  itemsPerPage = 20;

  ngOnInit(): void {
    this.searchCities('');
  }

  searchCities(searchTerm: string): void {
    this.searchTerm = searchTerm;
    const filtered = (this.searchTerm?.length > 0)
      ? GeographyApiService.FilterCities(this.searchTerm, this.geographyService.cities)
      : this.geographyService.cities;

    this.cities = filtered;
    this.updateDisplayedCities();
  }

  updateDisplayedCities(): void {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    const end = start + this.itemsPerPage;
    this.displayedCities = this.cities.slice(start, end);
  }

  onPageChange(event: any): void {
    this.currentPage = event.page;
    this.updateDisplayedCities();
  }

  saveCity(city: CityDetail = undefined): void {
    CityDialogComponent.showDialog(this.dialogBS, city).pipe(
      first(),
      untilDestroyed(this),
      switchMap(cd => cd?.id === -1 ? of(cd) : this._saveCity(cd))
    ).subscribe({
      next: (cd) => {
        if (cd) {
          if (cd?.id > 0) {
            this.geographyService.init()
              .pipe(untilDestroyed(this), filter(res => !!res))
              .subscribe(() => this.searchCities(this.searchTerm));

            this.popup.showSuccess('geography.success-save-city', { name: cd.name });
          }
        } else {
          this.popup.showError('users.error-save-city');
        }
      },
      error: err => {
        this.popup.showError(err.toString(), { name: city?.name });
      }
    });
  }

  _saveCity(city: CityDetail) {
    this.overlay.show();
    return this.geographyService.saveCity(city);
  }

  onDelete(city: CityDetail): void {
    MessageBoxComponent.show(this.dialogBS, {
      title: this.translate.instant('title.confirm'),
      message: this.translate.instant('geography.delete-city', { name: city.name })
    } as MessageBoxOptions)
      .pipe(untilDestroyed(this))
      .subscribe(res => {
        if (res) {
          this.overlay.show();
          this.geographyService.deleteCity(city.id)
            .pipe(untilDestroyed(this))
            .subscribe({
              next: () => {
                this.geographyService.init()
                  .pipe(untilDestroyed(this), filter(res => !!res))
                  .subscribe(() => this.searchCities(this.searchTerm));

                this.popup.showSuccess('geography.success-delete-city', { name: city.name });
              },
              error: err => {
                this.popup.showError(err.toString(), {
                  name: city.name,
                  subregion: `${city.regionName} > ${city.subregion}`
                });
              }
            });
        }
      });
  }
}