import { Component, ElementRef, OnInit, Renderer2 } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { MenuService } from 'src/app/services/menu.service';

@UntilDestroy()
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  showNavBar = true;

  constructor(
    private readonly menuService: MenuService,
    private el: ElementRef, 
    private renderer: Renderer2
  ) {
    this.menuService.showNavBar$
      .pipe(untilDestroyed(this))
      .subscribe(res => this.setNavBarVisibility(res));
  }

  ngOnInit() {
    this.setNavBarVisibility(this.menuService.showNavBar$.getValue());
  }

  private setNavBarVisibility(show: boolean) {
    this.showNavBar = show;
    this.renderer.setAttribute(this.el.nativeElement, 'show-nav-bar', show ? 'true' : 'false');
  }
}
