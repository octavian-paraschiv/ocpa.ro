import { Component, OnInit } from '@angular/core';
import { faCloudSunRain, faSquare, faMicrochip, faPhotoFilm, faPlay,
  faAngleRight } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
  faWeather = faCloudSunRain;
  faElectronics = faMicrochip;
  faSquare = faSquare;
  faPhoto = faPhotoFilm;
  faPlay = faPlay;
  faAngle = faAngleRight;

  ngOnInit(): void {
  }
}
