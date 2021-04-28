// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { DatePipe } from '@angular/common';
import { DateHelper } from './date.helper';

describe('DateHelper', () => {
  let dateHelper: DateHelper;
  let datePipe: DatePipe;

  beforeEach(() => {
    datePipe = new DatePipe('nl');
    dateHelper = new DateHelper('nl');

  });

  it('getDayAgo returns date on x days -2 ago', () => {
    const daysInPast = 2;
    // testdate = 31 dec 2020
    const inputDate = new Date(Date.UTC(2020, 0, 3, 4, 2, 2));

    const expectedDate = new Date(Date.UTC(2020, 0, 1, 0, 0, 0, 0));
    const result = dateHelper.getDayAgo(daysInPast, inputDate);

    expect(result).toEqual(expectedDate);
  });

  it('getDayAgo returns date on x days -2 ago ny eve', () => {
    const daysInPast = 2;
    // testdate = 31 dec 2020
    const inputDate = new Date(Date.UTC(2020, 0, 2, 4, 2, 2));

    const expectedDate = new Date(Date.UTC(2019, 11, 31, 0, 0, 0, 0));
    const result = dateHelper.getDayAgo(daysInPast, inputDate);

    expect(result).toEqual(expectedDate);
  });

  it('getDayAgo returns date on x days -2 ago gmt +1', () => {
    const daysInPast = 2;
    // testdate = 31 dec 2020
    const inputDate = new Date(Date.UTC(2020, 11, 31, 4, 2, 2));

    const expectedDate = new Date(Date.UTC(2020, 11, 29, 0, 0, 0, 0));
    const result = dateHelper.getDayAgo(daysInPast, inputDate);

    expect(result).toEqual(expectedDate);
  });

  it('getDayAgo returns date on x days -2 ago gmt +2', () => {
    const daysInPast = 2;
    // testdate = 6 sep 2020
    const inputDate = new Date(Date.UTC(2020, 6, 3, 4, 2, 2));

    const expectedDate = new Date(Date.UTC(2020, 6, 1, 0, 0, 0, 0));
    const result = dateHelper.getDayAgo(daysInPast, inputDate);

    expect(result).toEqual(expectedDate);
  });

  it('getDaysAgo returns 2 dagen gel.', () => {
    // testdate = 6 sep 2020
    const inputDate = dateHelper.getDayAgo(2, new Date());

    const expected = '2 dagen gel.';
    const result = dateHelper.getDaysAgo(inputDate);

    expect(result).toEqual(expected);
  });

  it('getDaysAgo returns 10 dagen gel.', () => {
    // testdate = 6 sep 2020
    const inputDate = dateHelper.getDayAgo(10, new Date());

    const expected = '10 dagen gel.';
    const result = dateHelper.getDaysAgo(inputDate);

    expect(result).toEqual(expected);
  });

  it('getDaysAgo returns 1 dag gel.', () => {
    // testdate = 6 sep 2020
    const inputDate = dateHelper.getDayAgo(1, new Date());

    const expected = '1 dag gel.';
    const result = dateHelper.getDaysAgo(inputDate);

    expect(result).toEqual(expected);
  });

  it('getDaysAgo returns vandaag', () => {
    // testdate = 6 sep 2020
    const inputDate = new Date();

    const expected = 'vandaag';
    const result = dateHelper.getDaysAgo(inputDate);

    expect(result).toEqual(expected);
  });

  it('getFriendlyDate returns date in format EE. d MMM - ', () => {
    const daysInPast = 2;
    // testdate = 6 sep 2020
    const inputDate = new Date(Date.UTC(2020, 6, 3, 4, 2, 2));

    const expectedDate = datePipe.transform(new Date(Date.UTC(2020, 6, 3, 4, 2, 2)), 'EE. d MMM - ');
    const result = dateHelper.getFriendlyDate(inputDate);

    expect(result).toEqual(expectedDate);
  });

  it('getFriendlyDateFull returns date in format EEEE d MMMM', () => {
    const daysInPast = 2;
    // testdate = 6 sep 2020
    const inputDate = new Date(Date.UTC(2020, 6, 3, 4, 2, 2));

    const expectedDate = datePipe.transform(new Date(Date.UTC(2020, 6, 3, 4, 2, 2)), 'EEEE d MMMM');
    const result = dateHelper.getFriendlyDateFull(inputDate);

    expect(result).toEqual(expectedDate);
  });

});
