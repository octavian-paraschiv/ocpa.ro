import { Component, OnDestroy, inject } from '@angular/core';
import { NavigationStart } from '@angular/router';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { Subscription } from 'rxjs';
import { first } from 'rxjs/operators';
import { BaseComponent } from 'src/app/components/base/BaseComponent';
import { Helper } from 'src/app/helpers/helper';
import { ContentApiService } from 'src/app/services/api/content-api.service';

@UntilDestroy()
@Component({
    selector: 'app-wiki-viewer',
    template: '<markdown katex emoji [data]="content"></markdown>'
})
export class WikiViewerComponent extends BaseComponent implements OnDestroy {
    content = 'n/a';
    private readonly contentService = inject(ContentApiService);
    private navSub: Subscription;

    constructor() {
        super();
        this.navSub = this.router.events.subscribe(event => {
            if (event instanceof NavigationStart) {
                this.reset();
            }
        });
    }

    public reset() {
        this.content = 'n/a';
        this.cleanupImages();
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
        const pageTitle = this.buildWikiPageTitle();
        if (pageTitle?.length > 0) {
            document.title = pageTitle;
        }

        const modalImageTitle = document.getElementById('modalImageTitle') as HTMLSpanElement;
        const imageModal = document.getElementById('imageModal');
        const modalImage = document.getElementById('modalImage') as HTMLImageElement;
        const imageModalcloseBtn = document.querySelector('.img-modal-close') as HTMLSpanElement;

        document.querySelectorAll('img')?.forEach((img: HTMLImageElement) => {
            img.setAttribute('loading', 'lazy');
        });

        document.querySelectorAll('.modal-popup-image')?.forEach(img => {
            const image = (img as HTMLImageElement);
            if (image) {
                const title = image.title;
                image.title = this.translate.instant('click-to-enlarge');
                image.addEventListener('click', () => {
                    imageModal.style.display = 'grid';
                    modalImage.src = image.src;
                    modalImageTitle.innerText = title;
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


    cleanupImages() {
        document.querySelectorAll('img')?.forEach((img: HTMLImageElement) => {
            img.src = '';
        });
    }

    buildWikiPageTitle() : string {
        for(let i = 1; i < 5; i++) {
            let collection = document.getElementsByTagName(`h${i}`);
            if (collection?.length > 0) {
                return (collection.item(0) as HTMLElement).innerText;
            }
        }
        return undefined;
    }


    ngOnDestroy() {
        this.navSub?.unsubscribe();
        this.cleanupImages();
    }

}