// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import {
  AfterViewInit,
  Component,
  ElementRef,
  HostListener,
  OnInit,
  ViewChild
} from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { LabConfirmService } from '../../../services/lab-confirm.service';
import { AppConfigService } from '../../../services/app-config.service';
import { DateHelper } from '../../../helpers/date.helper';
import { IndexData } from '../../../models/index-data';

@Component({
  selector: 'app-validate-step2',
  templateUrl: './validate-start-input.component.html',
  styleUrls: ['./validate-start-input.component.scss']
})
export class ValidateStartInputComponent implements OnInit, AfterViewInit {
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private reportService: LabConfirmService,
    private appConfigService: AppConfigService,
    public dateHelper: DateHelper
  ) {
    const config = this.appConfigService.getConfig();
    this.indexData = new IndexData(config.symptomaticIndexDayOffset, config.aSymptomaticIndexDayOffset, dateHelper);
  }

  // The person having a test or having symptoms or both contacting the GGD
  public indexData: IndexData;

  private LastConfirmedLCId: Array<string> = ['', '', '', '', '', '', ''];

  openDayPicker = false;
  demoMode = false;
  errorCode = -1;
  loading = 0;

  @ViewChild('first_char')
  first_char: ElementRef;

  @ViewChild('step_element')
  step_element: ElementRef;

  ngOnInit(): void {
  }
  ngAfterViewInit() {
  }

  @HostListener('window:scroll', ['$event'])
  scrollHandler(event) {
    const y = (this.step_element.nativeElement.offsetTop - window.outerHeight + 220);
    if (this.indexData.GGDKey.join('').length < 1 && window.scrollY > y) {
      const firstCharInputElement: HTMLInputElement = this.first_char.nativeElement;
      firstCharInputElement.focus();
    }
  }

  focusInput($event: FocusEvent) {
    const target = $event.target as HTMLInputElement;
    target.select();
    setTimeout(() => {
      target.select();
    }, 1); // safari webkit select issue
  }

  evaluateInvalidState($event: KeyboardEvent, index: number) {
    if (this.errorCode === 2) {
      for (let i = 0; i < 6; i++) {
        if (i !== index) {
          this.indexData.GGDKeyValidState[i] = null;
        }
      }
    }
    const labCICharacter = this.indexData.GGDKey[index];
    if (labCICharacter.length > 0) {
      const labCICharacterValidMatch = labCICharacter.toUpperCase().match('^[' + this.indexData.allowedChars + ']+$');
      this.indexData.GGDKeyValidState[index] = !(labCICharacterValidMatch == null || labCICharacterValidMatch.length < 1);
    } else {
      this.indexData.GGDKeyValidState[index] = true;
    }

    this.demoMode = (this.indexData.GGDKeyJoined() === '000000');
    if (!this.demoMode) {
      this.errorCode = (Object.values(this.indexData.GGDKeyValidState).filter(s => s === false).length > 0) ? 1 : -1;
    } else {
      this.errorCode = -1;
    }
  }

  focusOnNext(target: Element) {
    if ((target.nextElementSibling) instanceof HTMLInputElement) {
      const nextInput = (target.nextElementSibling) as HTMLInputElement;
      nextInput.focus();
    } else if (target.nextElementSibling !== null) {
      this.focusOnNext(target.nextElementSibling);
    }
  }

  focusOnPrev(target: Element) {
    if ((target.previousElementSibling) instanceof HTMLInputElement) {
      target.previousElementSibling.focus();
    } else if (target.previousElementSibling !== null) {
      this.focusOnPrev(target.previousElementSibling);
    }
  }

  icIdKeyPress($event: KeyboardEvent) {
    const target = $event.target as HTMLInputElement;
    const index = Array.prototype.indexOf.call(target.parentElement.children, target);

    if ($event.code === 'ArrowRight') {
      this.focusOnNext(target);
    } else if ($event.code === 'ArrowLeft') {
      this.focusOnPrev(target);
    } else if ($event.code === 'Backspace') {
      target.value = '';
      this.focusOnPrev(target);
    } else if ($event.code === 'Enter') {
      this.openDayPicker = true;
    } else {
      if ([...target.value].length > 1) {
        target.value = target.value[target.value.length - 1]; // prevent fast overfilling
      }
      if (target.value.length > 0) {
        target.value = target.value.toUpperCase();
        this.focusOnNext(target);
      }
    }
    // sync with model
    this.indexData.GGDKey[index] = target.value;
  }

  hasSymptomsClicked(symptomatic: boolean) {
    this.indexData.isSymptomatic(symptomatic);
  }

  selectDateClick(dateDay: number) {
    this.indexData.selectedDate = this.dateHelper.getDayAgo(dateDay);
    this.openDayPicker = false;
  }

  confirmGGDKey() {
    if (this.indexData.GGDKeyJoined() === '000000') {
      this.router.navigate(['/validate/confirm'], {
        queryParams: {
          symptomsDate: this.indexData.getDisplayDate().valueOf()
        }
      });
    }

    if (this.indexData.GGDKey.join('') === this.LastConfirmedLCId.join('')) {
      alert('Het is niet mogelijk om meerdere keren dezelfde GGD-sleutel te verwerken. Probeer het opnieuw met een unieke GGD-sleutel.');
      return;
    }
    if (this.indexData.InfectionConfirmationIdValid()) {
      this.loading++;

      this.reportService.confirmLabId(
        this.indexData.GGDKey,
        this.indexData.selectedDate.toISOString(),
        this.indexData.symptomatic
      )
        .pipe(catchError((e) => {
          this.loading--;
          this.errorCode = 2;
          throw e;
        })).subscribe((result) => {
          this.loading--;
          this.LastConfirmedLCId = [...this.indexData.GGDKey]; // 200 response

          if (result.valid === true) {
            this.router.navigate(['/validate/confirm'], {
              queryParams: {
                symptomsDate: this.indexData.getDisplayDate().valueOf()
              }
            });
          } else {
            this.errorCode = 2;
          }
          if (this.errorCode > 1) {
            for (let i = 0; i < this.indexData.GGDKey.length; i++) {
              this.indexData.GGDKeyValidState[i] = false;

              if (i === 6 && this.indexData.GGDKey[i] === '') {
                this.indexData.GGDKeyValidState[i] = null;
              }
            }
          }
        });
    } else {
      this.errorCode = 1;
    }
  }
}
