import { Directive, HostListener, ElementRef } from '@angular/core';

@Directive({
  selector: '[inputMaskFormat]'
})
export class InputMaskFormatDirective {
  private regex: RegExp = /^[A-Za-z0-9]{0,4}-?[A-Za-z0-9]{0,4}$/;

  constructor(private el: ElementRef) {}

  @HostListener('input', ['$event'])
  onInput(event: Event): void {
    const input = this.el.nativeElement;
    let value: string = input.value.toUpperCase().replace(/[^A-Z0-9]/g, '');

    if (value.length > 4) {
      value = value.slice(0, 4) + '-' + value.slice(4, 8);
    }

    input.value = value;
  }

  @HostListener('blur')
  onBlur(): void {
    const input = this.el.nativeElement;
    const value = input.value;

    if (!this.regex.test(value.replace('-', ''))) {
      input.setCustomValidity('Code must be in format AAAA-AAAA');
    } else {
      input.setCustomValidity('');
    }
  }
}