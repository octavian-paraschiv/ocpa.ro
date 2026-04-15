import { NgModule } from '@angular/core';
import { ModalModule } from 'ngx-bootstrap/modal';
import { PaginationModule } from 'ngx-bootstrap/pagination';

// *************** FORM CONTROLS ***************

// *************** NAVIGATION ***************

// *************** LAYOUT ***************

// *************** BUTTONS & INDICATORS ***************

// *************** POPUPS & MODALS ***************

// *************** DATA TABLE ***************

const BootstrapModules = [
  ModalModule.forRoot(),
  PaginationModule.forRoot(),
];

@NgModule({
  imports: [BootstrapModules],
  exports: [BootstrapModules],
})
export class BootstrapModule {}