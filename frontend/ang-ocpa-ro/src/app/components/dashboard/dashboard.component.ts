import { Component, OnDestroy, OnInit } from "@angular/core";
import { UntilDestroy } from "@ngneat/until-destroy";

@UntilDestroy()
@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent
implements OnInit, OnDestroy {

  ngOnInit() {

  }

  ngOnDestroy() {

  }
}
