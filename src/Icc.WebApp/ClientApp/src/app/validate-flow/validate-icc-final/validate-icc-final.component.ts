// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { Component, Inject, LOCALE_ID, OnInit } from '@angular/core';
import {ActivatedRoute} from '@angular/router';
import {DatePipe} from '@angular/common';

@Component({
    selector: 'app-validate-icc-final',
    templateUrl: './validate-icc-final.component.html',
    styleUrls: ['./validate-icc-final.component.scss']
})
export class ValidateIccFinalComponent implements OnInit {
    public success = false;
    public symptomsDate: Date;
    private datePipe: DatePipe;

    constructor(@Inject(LOCALE_ID) private locale: string, private route: ActivatedRoute) {
        this.datePipe = new DatePipe(locale);
        if (route.snapshot.queryParams.success) {
            this.success = route.snapshot.queryParams.success;
            window.history.pushState(null, null, 'validate_final');
        }
        if (route.snapshot.queryParams.symptomsDate) {
            const queryParamSymptomsDate = parseInt(route.snapshot.queryParams.symptomsDate, 0);
            if (Number.isInteger(queryParamSymptomsDate)) {
                this.symptomsDate = new Date(queryParamSymptomsDate);
            }
        }
    }

    public friendlySymptomsDate(offset: number = 0): string {
        if (!this.symptomsDate) {
            return '';
        }
        const date = new Date(this.symptomsDate);
        date.setDate(date.getDate() - offset);
        return this.datePipe.transform(date, 'EEEE d MMMM');
    }

    ngOnInit(): void {
    }

}
