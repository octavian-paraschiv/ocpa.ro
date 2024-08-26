import { Component } from '@angular/core';
import { ActivationStart, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthenticationService } from 'src/app/services/authentication.services';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'app';
  path: string = '/admin';

  constructor(private readonly authService: AuthenticationService,
    private readonly router: Router) {
      this.router.events
            .pipe(filter(e => e instanceof ActivationStart))
            .subscribe(e => {
                try { 
                  this.title = (e as ActivationStart).snapshot.data['title']; 
                  const path = (e as ActivationStart).snapshot.routeConfig.path;
                  if (path && path.length > 0) {
                    this.path = path;
                  } else {
                    this.path = 'ocpa';
                  }
                }
                catch { }
            });
  }

  get isAdminMode(): boolean {
    return this.isAdminPath && this.authService.validAdminUser;
  }

  get isAdminPath() {
    return this.path?.startsWith('/admin');
  }
}
