import { Injectable } from '@angular/core';
import { OverlayComponent } from 'src/app/components/shared/overlay/overlay.component';

@Injectable({ providedIn: 'root' }) 
export class OverlayService { 
    private overlayComponent: OverlayComponent;

    constructor() { }
    
    setOverlayComponent(overlayComponent: OverlayComponent) { 
        this.overlayComponent = overlayComponent; 
    }

    show() { this.overlayComponent.show(); }
    hide() { this.overlayComponent.hide(); } 
}
    