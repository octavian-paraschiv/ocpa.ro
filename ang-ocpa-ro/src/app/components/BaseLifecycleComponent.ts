import { AfterViewInit, Component, OnDestroy, OnInit } from '@angular/core';

@Component({ template: '' })
export abstract class BaseLifecycleComponent implements OnInit, OnDestroy, AfterViewInit {
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
