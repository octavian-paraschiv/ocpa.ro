import { Component, OnDestroy, OnInit } from "@angular/core";
import { UntilDestroy } from "@ngneat/until-destroy";

@UntilDestroy()
@Component({
  selector: 'app-player',
  templateUrl: './player.component.html'
})
export class PlayerComponent
implements OnInit, OnDestroy {

  ngOnInit() {

  }

  ngOnDestroy() {

  }
}
