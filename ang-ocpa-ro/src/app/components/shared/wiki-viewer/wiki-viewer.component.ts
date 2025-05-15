import { Component, Input, inject } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { TranslateService } from '@ngx-translate/core';
import { first } from 'rxjs/operators';
import { BaseComponent } from 'src/app/components/base/BaseComponent';
import { Helper } from 'src/app/helpers/helper';
import { ContentApiService } from 'src/app/services/api/content-api.service';

@UntilDestroy()
@Component({
    selector: 'app-wiki-viewer',
    template: '<markdown katex emoji [data]="content"></markdown>'
})
export class WikiViewerComponent extends BaseComponent {
    content = 'n/a';
    private readonly contentService = inject(ContentApiService);

    public reset() {
        this.content = 'n/a';
    }

    public displayLocation(location: string, renderTranslated: boolean) {
        this.overlay.show();
        this.contentService
            .renderContent(location, renderTranslated)
            .pipe(first(), untilDestroyed(this))
            .subscribe({
                next: res => { 
                    this.overlay.hide();
                    this.content = res ?? this.translate.instant('wiki.default-content');
                    if (!Helper.isMobile()) {
                        setTimeout(() => this.setUpImageModals(), 500);
                    }
                },
                error: err => {
                    this.overlay.hide();
                    this.content = err.toString();
                }
            });
    }

    setUpImageModals() {
        const imageModal = document.getElementById('imageModal');
        const modalImage = document.getElementById('modalImage') as HTMLImageElement;
        const imageModalcloseBtn = document.querySelector('.img-modal-close') as HTMLSpanElement;

        document.querySelectorAll('.modal-popup-image').forEach(img => {
            const image = (img as HTMLImageElement);
            if (image) {
                image.title = this.translate.instant('click-to-enlarge');
                image.addEventListener('click', () => {
                    imageModal.style.display = 'block';
                    modalImage.src = (img as HTMLImageElement).src;
                });
            }
        });

        imageModalcloseBtn.onclick = () => {
            imageModal.style.display = 'none';
        };

        imageModal.onclick = (event) => {
            if (event.target === imageModal) {
                imageModal.style.display = 'none';
            }
        };
    }

    getText(base64: string): string {
        try {
          // Decode the Base64 string
          const decodedBytes = window.atob(base64);
   
          if (decodedBytes?.length > 0 && decodedBytes.length < 1024 * 1024) {
           // Convert the decoded bytes to a string
           const decodedString = new TextDecoder().decode(new Uint8Array(decodedBytes.split('').map(char => char.charCodeAt(0))));
          
           // Check if the string contains valid UTF-8 characters
           if (/^[\x00-\x7F]*$/.test(decodedString))
             return decodedString;
         }
        } catch {
          // If decoding fails, it's likely binary data
        }
        return undefined;
     }  
}