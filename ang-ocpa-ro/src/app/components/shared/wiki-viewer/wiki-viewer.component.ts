import { Component, inject } from '@angular/core';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { TranslateService } from '@ngx-translate/core';
import { first } from 'rxjs/operators';
import { ContentApiService } from 'src/app/services/api/content-api.service';

@UntilDestroy()
@Component({
    selector: 'app-wiki-viewer',
    template: '<markdown katex emoji [data]="content"></markdown>'
})
export class WikiViewerComponent {
    content = 'n/a';
    private readonly translate = inject(TranslateService);
    private readonly contentService = inject(ContentApiService);

    public displayLocation(location: string) {
        this.contentService
            .renderContent(location)
            .pipe(first(), untilDestroyed(this))
            .subscribe({
                next: res => this.content = res ?? this.translate.instant('wiki.default-content'),
                error: err => this.content = err.toString(),
            });
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