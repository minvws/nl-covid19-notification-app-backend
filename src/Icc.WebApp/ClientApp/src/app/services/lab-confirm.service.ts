// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { AuthenticationService } from './authentication.service';
import { AppConfigService} from './app-config.service';


@Injectable({
  providedIn: 'root'
})
export class LabConfirmService {
  constructor(private readonly http: HttpClient,
    private readonly authenticationService: AuthenticationService,
    private readonly appConfigService: AppConfigService) {
    }

  private data: { GGDKey: string; SelectedDate: string; Symptomatic: boolean; };

  private static errorHandler(error: HttpErrorResponse, caught: Observable<any>): Observable<any> {
    // TODO error handling
    throw error;
  }

  confirmLabId(GGDKeys: Array<string>, selectedDate: string, symptomatic: boolean): Observable<any> {
    const serviceUrl = location.origin + '/pubtek';
    this.data = {
      'GGDKey': GGDKeys.join(''),
      'SelectedDate': selectedDate,
      'Symptomatic': symptomatic
    };
    const headers = {
      headers: {
        'Authorization': 'Bearer ' + this.authenticationService.currentUserValue.authData
      }
    };

    return this.http.put(serviceUrl, this.data, headers).pipe(catchError(LabConfirmService.errorHandler));
  }
}
