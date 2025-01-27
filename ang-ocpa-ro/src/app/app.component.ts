import { Component, ElementRef, OnInit, Renderer2 } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { MenuService } from 'src/app/services/menu.service';

@UntilDestroy()
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  constructor(
    private readonly menuService: MenuService,
    private el: ElementRef, 
    private renderer: Renderer2
  ) {
    this.menuService.singleMenuApp$
      .pipe(untilDestroyed(this))
      .subscribe(res => this.setSingleMenuApp(res));
  }

  ngOnInit() {
    this.setSingleMenuApp(this.menuService.singleMenuApp$.getValue());
  }

  private setSingleMenuApp(singleMenuApp: boolean) {
    this.renderer.setAttribute(this.el.nativeElement, 'single-menu-app', singleMenuApp ? 'true' : 'false');
  }
}
