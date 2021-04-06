// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import {
  AfterViewInit,
  Component,
  ElementRef,
  HostListener,
  Inject,
  LOCALE_ID,
  OnInit,
  ViewChild
} from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { LabConfirmService } from '../../../services/lab-confirm.service';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-validate-step2',
  templateUrl: './validate-start-input.component.html',
  styleUrls: ['./validate-start-input.component.scss']
})
export class ValidateStartInputComponent implements OnInit, AfterViewInit {

  public LabConfirmationId: Array<string> = ['', '', '', '', '', '', ''];
  private LastConfirmedLCId: Array<string> = ['', '', '', '', '', '', ''];
  public LabConfirmationIdValidState: { [key: number]: boolean } = [];
  @ViewChild('first_char')
  first_char: ElementRef;

  @ViewChild('step_element')
  step_element: ElementRef;
  error_code = -1;
  allowedChars = 'BCFGJLQRSTUVXYZ23456789';
  loading = 0;

  // datepart
  showSymptoms = true;

  private todayDate: Date = new Date();
  symptomsDate: Date = null;
  datePipe: DatePipe;
  openDayPicker = false;
  dateArray: Array<number> = [...Array(22)];

  demoMode = false;

  spelAlphabet: Object = {
    'A': 'Anna',
    'B': 'Bernard',
    'C': 'Cornelis',
    'D': 'Dirk',
    'E': 'Eduard',
    'F': 'Ferdinand',
    'G': 'Gerard',
    'H': 'Hendrik',
    'I': 'Izak',
    'J': 'Jan',
    'K': 'Karel',
    'L': 'Lodewijk',
    'M': 'Maria',
    'N': 'Nico',
    'O': 'Otto',
    'P': 'Pieter',
    'Q': 'Quotiënt',
    'R': 'Rudolf',
    'S': 'Simon',
    'T': 'Teunis',
    'U': 'Utrecht',
    'V': 'Victor',
    'W': 'Willem',
    'X': 'Xantippe',
    'Y': 'Y-grec',
    'Z': 'Zaandam'
  };

  constructor(
    @Inject(LOCALE_ID) private locale: string,
    private route: ActivatedRoute,
    private router: Router,
    private reportService: LabConfirmService) {
    this.datePipe = new DatePipe(locale);
    for (let i = 0; i < 6; i++) {
      this.LabConfirmationIdValidState[i] = null;
    }
  }

  ngAfterViewInit() {
  }

  @HostListener('window:scroll', ['$event'])
  scrollHandler(event) {
    const y = (this.step_element.nativeElement.offsetTop - window.outerHeight + 220);
    if (this.LabConfirmationId.join('').length < 1 && window.scrollY > y) {
      const firstCharInputElement: HTMLInputElement = this.first_char.nativeElement;
      firstCharInputElement.focus();
    }
  }

  ngOnInit(): void {
  }

  public InfectionConfirmationIdValid() {
    return (this.labConfirmationIdJoined().length >= 6 && this.validateCharacters());
  }

  public InfectionConfirmationIdToTaalString() {
    let output = '';
    this.LabConfirmationId.forEach((c, index) => {
      if (index === 3) {
        output += ' – ';
      }
      if (this.spelAlphabet[c]) {
        output += '<b>' + c + '</b>' + ' (' + this.spelAlphabet[c] + ')';
      } else {
        output += '<b>' + c + '</b>';
      }
      output += ' ';
    });
    return output;
  }

  labConfirmationIdJoined() {
    return this.LabConfirmationId.join('').trim().toUpperCase();
  }

  validateCharacters(): boolean {
    const matchArray: RegExpMatchArray = this.labConfirmationIdJoined().match('^[' + this.allowedChars + ']+$');
    return matchArray && matchArray.length > 0;
  }

  focusInput($event: FocusEvent) {
    const target = $event.target as HTMLInputElement;
    target.select();
    setTimeout(() => {
      target.select();
    }, 1); // safari webkit select issue
  }

  evaluateInvalidState($event: KeyboardEvent, index: number) {
    if (this.error_code === 2) {
      for (let i = 0; i < 6; i++) {
        if (i !== index) {
          this.LabConfirmationIdValidState[i] = null;
        }
      }
    }
    const labCICharacter = this.LabConfirmationId[index];
    if (labCICharacter.length > 0) {
      const labCICharacterValidMatch = labCICharacter.toUpperCase().match('^[' + this.allowedChars + ']+$');
      this.LabConfirmationIdValidState[index] = !(labCICharacterValidMatch == null || labCICharacterValidMatch.length < 1);
    } else {
      this.LabConfirmationIdValidState[index] = true;
    }

    this.demoMode = (this.labConfirmationIdJoined() === '000000');
    if (!this.demoMode) {
      this.error_code = (Object.values(this.LabConfirmationIdValidState).filter(s => s === false).length > 0) ? 1 : -1;
    } else {
      this.error_code = -1;
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
    let index = Array.prototype.indexOf.call(target.parentElement.children, target);

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
    this.LabConfirmationId[index] = target.value;
  }

  getFriendlySymptomsDate(format: string = 'EE. d MMM - ', offset: number = 0) {
    let date = this.symptomsDate;
    if (date) {
      date = new Date(this.symptomsDate.valueOf());
      date.setDate(date.getDate() - offset);
    }
    return this.datePipe.transform(date, format);
  }

  getDaysAgo(inputDate: Date = null): string {
    inputDate = ((inputDate != null) ? inputDate : this.symptomsDate);
    const daysAgo = (inputDate) ? Math.floor(((Date.now() - (inputDate).valueOf()) / 1000 / 60 / 60 / 24)) : 0;
    return (daysAgo < 1) ? 'vandaag' : (daysAgo + ' ' + ((daysAgo > 1) ? 'dagen' : 'dag') + ' gel.');
  }

  getDayAgo(dayCount: number, inputDate: Date = null): Date {

    if (inputDate == null) {
      inputDate = this.todayDate;
    }

    const startOfDay = new Date(
      Date.UTC(
        inputDate.getUTCFullYear(),
        inputDate.getUTCMonth(),
        inputDate.getUTCDate(),
        0,
        0,
        0,
        0
      )
    );

    if (dayCount > 0) {
      return new Date(startOfDay.setDate(startOfDay.getDate() - dayCount));
    }
    return startOfDay;
  }

  selectDate(dateDay: number) {
    this.symptomsDate = this.getDayAgo(dateDay);
    this.openDayPicker = false;
  }

  confirmLabConfirmationId() {
    if (this.labConfirmationIdJoined() === '000000') {
      this.router.navigate(['/validate/confirm'], {
        queryParams: {
          p: `demo_polltoken_test_000000`,
          symptomsDate: this.symptomsDate.valueOf()
        }
      });
    }

    if (this.LabConfirmationId.join('') === this.LastConfirmedLCId.join('')) {
      alert('Het is niet mogelijk om meerdere keren dezelfde GGD-sleutel te verwerken. Probeer het opnieuw met een unieke GGD-sleutel.');
      return;
    }
    if (this.InfectionConfirmationIdValid()) {
      this.loading++;
      this.reportService.confirmLabId(this.LabConfirmationId, this.symptomsDate.toISOString())
        .pipe(catchError((e) => {
          this.loading--;
          this.error_code = 2;
          throw e;
        })).subscribe((result) => {
          this.loading--;
          this.LastConfirmedLCId = [...this.LabConfirmationId]; // 200 response

          if (result.valid === true) {
            this.router.navigate(['/validate/confirm'], {
              queryParams: {
                p: result.pollToken,
                symptomsDate: this.symptomsDate.valueOf()
              }
            });
          } else {
            this.error_code = 2;
          }
        if (this.error_code > 1) {
          for (let i = 0; i < this.LabConfirmationId.length; i++) {
            this.LabConfirmationIdValidState[i] = false;

            if (i === 6 && this.LabConfirmationId[i] === '') {
              this.LabConfirmationIdValidState[i] = null;
            }
          }
        }
      });
    } else {
      this.error_code = 1;
    }
  }
}
