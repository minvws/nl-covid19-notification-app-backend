// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { Component, Inject, LOCALE_ID, OnInit } from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {DatePipe} from '@angular/common';

@Component({
    selector: 'app-validate-icc-confirm',
    templateUrl: './validate-icc-confirm.component.html',
    styleUrls: ['./validate-icc-confirm.component.scss']
})
export class ValidateIccConfirmComponent implements OnInit {
    public success = false;
    public symptomsDate: number;
    private datePipe: DatePipe;

    constructor(@Inject(LOCALE_ID) private locale: string, private router: Router, private route: ActivatedRoute) {
        this.datePipe = new DatePipe(locale);
        if (route.snapshot.queryParams.symptomsDate) {
            window.history.pushState(null, null, 'validate/confirm');
            this.symptomsDate = parseInt(route.snapshot.queryParams.symptomsDate, 0);
        }
    }

    ngOnInit(): void {
    }

    backToStart() {
        if (confirm('Weet je zeker dat je deze index wilt annuleren?')) {
            this.router.navigate(['/validate/start']);
        }
    }

    goToValidateFinal() {
        this.router.navigate(['/validate_final'], {
            queryParams: {
                success: true,
                symptomsDate: this.symptomsDate
            }
        });
    }

}
