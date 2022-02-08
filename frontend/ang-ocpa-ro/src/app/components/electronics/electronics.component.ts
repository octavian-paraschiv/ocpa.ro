import { Component, OnDestroy, OnInit } from "@angular/core";
import { UntilDestroy } from "@ngneat/until-destroy";

@UntilDestroy()
@Component({
  selector: 'app-electronics',
  templateUrl: './electronics.component.html'
})
export class ElectronicsComponent
implements OnInit, OnDestroy {

  ngOnInit() {

  }

  ngOnDestroy() {

  }
}
