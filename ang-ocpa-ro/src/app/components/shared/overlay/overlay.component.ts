import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-overlay',
  templateUrl: './overlay.component.html'
})
export class OverlayComponent implements OnInit {
  isVisible = false;

  constructor() { }

  ngOnInit(): void { }

  show() {
    this.isVisible = true;
  }

  hide() {
    this.isVisible = false;
  }
}
