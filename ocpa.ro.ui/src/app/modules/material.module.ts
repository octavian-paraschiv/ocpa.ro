import { NgModule } from '@angular/core';

// *************** FORM CONTROLS ***************
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';

/*
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core'; //FROM ANGULAR CORE

import { MatRadioModule } from '@angular/material/radio';

import { MatSliderModule } from '@angular/material/slider';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
*/

// *************** NAVIGATION ***************
/*
import { MatMenuModule } from '@angular/material/menu';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
*/

// *************** LAYOUT ***************

import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatTreeModule } from '@angular/material/tree';
/*
import { MatExpansionModule } from '@angular/material/expansion';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatListModule } from '@angular/material/list';
import { MatStepperModule } from '@angular/material/stepper';
import { MatTabsModule } from '@angular/material/tabs';

*/

// *************** BUTTONS & INDICATORS ***************
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';

/*
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatBadgeModule } from '@angular/material/badge';


import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatRippleModule } from '@angular/material/core';
*/

// *************** POPUPS & MODALS ***************
import { MatDialogModule } from '@angular/material/dialog';

/*
import { MatBottomSheetModule } from '@angular/material/bottom-sheet';

import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
*/

// *************** DATA TABLE ***************
import { MatSelectFilterModule } from '@devlukaszmichalak/mat-select-filter';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';
/*
import { MatSortModule } from '@angular/material/sort';
*/

const MaterialModules = [
  MatCardModule,
  MatDividerModule,
  MatFormFieldModule,
  MatCheckboxModule,
  MatPaginatorModule,
  MatTableModule,
  MatTreeModule,
  MatSelectModule,
  MatIconModule,
  MatInputModule,
  MatButtonModule,
  MatChipsModule,

  /*
  MatAutocompleteModule,
  MatDatepickerModule,
  MatNativeDateModule,
  MatRadioModule,
  
  MatSliderModule,
  MatSlideToggleModule,
  MatMenuModule,
  MatSidenavModule,
  MatToolbarModule,
  MatExpansionModule,
  MatGridListModule,
  MatListModule,
  MatStepperModule,
  MatTabsModule,
  
  MatButtonToggleModule,
  MatBadgeModule,
  
  MatProgressSpinnerModule,
  MatProgressBarModule,
  MatRippleModule,
  MatBottomSheetModule,
  MatSnackBarModule,
  MatTooltipModule,
  MatSortModule,
  */
  MatSelectFilterModule
];

@NgModule({
  imports: [MaterialModules],
  exports: [MaterialModules],
})
export class MaterialModule {}