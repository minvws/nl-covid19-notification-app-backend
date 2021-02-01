// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { Component, OnInit } from '@angular/core';
import {TitleService} from '../services/title.service';
import {ActivatedRoute, Router} from '@angular/router';
import {AuthenticationService} from '../services';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

    public error_code: number;

    public constructor(private route: ActivatedRoute,
        private router: Router,
        public titleService: TitleService,
        private authentication: AuthenticationService) {
        titleService.setTitle('Home');
    }

    ngOnInit(): void {
        if (this.authentication.currentUserValue) {
            this.router.navigate(['validate/start']);
        }
        if (this.route.snapshot.queryParams['e']) {
            this.error_code = 1;
            history.pushState('', '', '/');
        }
    }

    authorize() {
        this.authentication.redirectToAuthorization();
    }
}
