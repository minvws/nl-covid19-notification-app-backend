// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import {
  Inject,
  Injectable,
  LOCALE_ID
} from '@angular/core';
import { DatePipe } from '@angular/common';

@Injectable()
export class DateHelper {

  constructor(@Inject(LOCALE_ID) private locale: string) {
    this.datePipe = new DatePipe(locale);
  }

  datePipe: DatePipe;

  private todayDate: Date = new Date();

  dateArray: Array<number> = [...Array(22)];

  addDays(date, days) {
    const result = new Date(date);
    result.setDate(result.getDate() + days);
    return result;
  }

  getFriendlyDate(date: Date, format: string = 'EE. d MMM - ') {
    return this.datePipe.transform(date, format);
  }

  getFriendlyDateFull(date: Date, format: string = 'EEEE d MMMM') {
    return this.datePipe.transform(date, format);
  }

  getDaysAgo(date: Date = null): string {
    const daysAgo = (date) ? Math.floor(((Date.now() - (date).valueOf()) / 1000 / 60 / 60 / 24)) : 0;
    return (daysAgo < 1) ? 'vandaag' : (daysAgo + ' ' + ((daysAgo > 1) ? 'dagen' : 'dag') + ' gel.');
  }

  getDayAgo(dayCount: number, date: Date = null): Date {
    if (date == null) {
      date = this.todayDate;
    }

    const startOfDay = new Date(
      Date.UTC(
        date.getUTCFullYear(),
        date.getUTCMonth(),
        date.getUTCDate(),
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
}
