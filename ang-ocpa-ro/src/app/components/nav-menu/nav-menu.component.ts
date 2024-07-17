import { Component } from '@angular/core';
import { Router, ActivationStart } from '@angular/router';
import { filter } from 'rxjs/operators';
import { faCloudSunRain, faSquare, faMicrochip, faPhotoFilm, faPlay,
  faAngleRight, faEarth } from '@fortawesome/free-solid-svg-icons';
import { AuthenticationService } from 'src/app/services/authentication.services';
import { UserType } from 'src/app/models/user';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html'
})
export class NavMenuComponent {
  faWeather = faCloudSunRain;
  faElectronics = faMicrochip;
  faSquare = faSquare;
  faPhoto = faPhotoFilm;
  faPlay = faPlay;
  faAngle = faAngleRight;
  faEarth = faEarth;

    title = 'OcPa\'s Web Site';
    path: string = 'ocpa';

    constructor(private readonly router: Router,
      private readonly authService: AuthenticationService) {
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
    return this.path?.startsWith('/admin') ||
      this.path?.startsWith('admin');
  }

  get isLoginPath() {
    return this.path?.startsWith('/login') ||
      this.path?.startsWith('login');
  }

  get isNonAdminPath() {
    return !this.isAdminMode && !this.isLoginPath;
  }

  logout() {
    this.authService.logout(true);
  }

  enterAdminMode() {
    this.router.navigate(['/admin']);
  }
}
