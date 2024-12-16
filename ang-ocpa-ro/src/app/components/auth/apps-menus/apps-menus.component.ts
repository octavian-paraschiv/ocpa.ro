import { Component } from '@angular/core';
import { UntilDestroy } from '@ngneat/until-destroy';
import { BaseAuthComponent } from 'src/app/components/auth/base/BaseAuthComponent';

@UntilDestroy()
@Component({
    selector: 'apps-menus',
    templateUrl: './apps-menus.component.html'
})
export class AppsMenusComponent extends BaseAuthComponent {
}