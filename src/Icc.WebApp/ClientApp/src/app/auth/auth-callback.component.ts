// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {AuthenticationService} from '../services';

@Component({
    selector: 'app-auth',
    templateUrl: './auth-callback.component.html',
    styleUrls: ['./auth-callback.component.scss']
})
export class AuthCallbackComponent implements OnInit {

    public error_code = -1;

    constructor(private route: ActivatedRoute, private authentication: AuthenticationService, private router: Router) {
    }

    ngOnInit(): void {
        this.route.queryParams.subscribe(async params => {

            if (params['code']) {
                this.authentication.callback(params['code']).subscribe((valid) => {
                    if (valid) {
                        this.router.navigate(['validate/start']);
                    } else {
                        this.error_code = 1;
                        this.router.navigate([''], {queryParams: {e: 'access_token_invalid'}});
                    }
                });
            }
        });
    }
}
