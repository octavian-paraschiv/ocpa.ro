import { Component, OnInit } from '@angular/core';
import { UntypedFormGroup, UntypedFormBuilder, Validators } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { fas } from '@fortawesome/free-solid-svg-icons';
import { UntilDestroy } from '@ngneat/until-destroy';
import { Observable, of, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { EMenuDisplayMode, Menu } from 'src/app/models/models-swagger';

@UntilDestroy()
@Component({
  selector: 'menu-dialog',
  templateUrl: './menu-dialog.component.html'
})
export class MenuDialogComponent implements OnInit {
  menuForm: UntypedFormGroup;
  editMode = false;
  displayModes = Object.keys(EMenuDisplayMode);
  menuIcons = Object.keys(fas);
  allIcons = fas;
  size = 'grow-3';
  selIcon: string = undefined;
  dropDownFocused = false;

  filterValue = '';
  filteredIcons = this.menuIcons.slice();

  menu: Menu;
  result$: Subject<Menu> = new Subject<Menu>();

  constructor(
    private formBuilder: UntypedFormBuilder,
    public bsModalRef: BsModalRef
  ) {}

  get f() {
    return this.menuForm?.controls;
  }

  get title(): string {
    return this.editMode ? 'menu-dialog.edit' : 'menu-dialog.create';
  }

  ngOnInit(): void {
    this.editMode = this.menu?.id > 0;

    if (!this.menu) {
      this.menu = {
        name: '',
        url: '',
        menuIcon: this.menuIcons[this.menuIcons.length - 1],
        displayModeId: Object.keys(EMenuDisplayMode).indexOf(EMenuDisplayMode.AlwaysHide)
      } as Menu;
    }

    if (this.menu.builtin) return;

    this.selIcon = this.menu.menuIcon;

    this.menuForm = this.formBuilder.group({
      name: [this.menu.name, [Validators.required, Validators.pattern('^.{3,24}$')]],
      url: [this.menu.url, [Validators.required, Validators.pattern('^\/[a-zA-Z0-9/._-]{2,127}$')]],
      displayMode: [Object.keys(EMenuDisplayMode)[this.menu.displayModeId]],
      menuIcon: [this.selIcon]
    });
  }

  onCancel(): void {
    this.bsModalRef.hide();
    this.result$.next({ id: -1 } as Menu);
    this.result$.complete();
    
  }

  onOk(): void {
    if (this.menuForm.invalid) return;

    this.bsModalRef.hide();
        this.result$.next({
            id: this.menu.id,
            name: this.f?.name?.value,
            url: this.f?.url?.value,
            displayModeId: Object.keys(EMenuDisplayMode).indexOf(this.f?.displayMode?.value),
            menuIcon: this.f?.menuIcon?.value,
            builtin: false
        } as Menu);
    this.result$.complete();
  }

  onDropDownFocused(focused: boolean) {
    this.dropDownFocused = focused;
  }

  static showDialog(modalService: BsModalService, menu?: Menu): Observable<Menu> {
      const bsModalRef: BsModalRef<MenuDialogComponent> = modalService.show(MenuDialogComponent, {
        initialState: { menu },
        class: 'bs-modal'      
      });
  
      return bsModalRef.content.result$.pipe(map(result => {
        return result ?? { id: -1 } as Menu;
      }));
  }
}