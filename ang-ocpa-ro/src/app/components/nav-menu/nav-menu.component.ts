import { Component } from '@angular/core';
import { Router, ActivationStart } from '@angular/router';
import { filter } from 'rxjs/operators';
import { faCloudSunRain, faSquare, faMicrochip, faPhotoFilm, faPlay,
  faAngleRight, faEarth } from '@fortawesome/free-solid-svg-icons';

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

    isExpanded = false;
    title = 'OcPa\'s Web Site';
    path: string = 'ocpa';

    constructor(private readonly router: Router) {
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

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
