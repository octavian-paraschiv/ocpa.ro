import { AfterViewInit, Component, OnDestroy, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Component({ template: '' })
export abstract class BaseLifecycleComponent implements OnInit, OnDestroy, AfterViewInit {
    constructor(protected translate: TranslateService) {
    }

    ngOnInit() {
        this.onInit();
    }

    ngOnDestroy(): void {
        this.onDestroy();
    }   

    ngAfterViewInit(): void {
        this.onAfterViewInit();
    }

    protected onInit() {}
    protected onDestroy() {}
    protected onAfterViewInit() {}
}
