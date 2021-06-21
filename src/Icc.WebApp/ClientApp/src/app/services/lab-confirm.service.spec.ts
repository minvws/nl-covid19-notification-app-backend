// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { TestBed } from '@angular/core/testing';
import {LabConfirmService} from './lab-confirm.service';
import {AppConfigService} from './app-config.service';
import {HttpClientTestingModule, HttpTestingController} from '@angular/common/http/testing';
import {RouterTestingModule} from '@angular/router/testing';
import {AppConfigTestService} from './app-config.test.service';
import {AuthenticationService} from './authentication.service';
import {AuthenticationTestService} from './authentication_test.service';

describe('LabConfirmServiceService', () => {
    let service: LabConfirmService;
    let httpTestingController: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule, RouterTestingModule],
            providers: [LabConfirmService, {
                provide: AppConfigService,
                useClass: AppConfigTestService,
            }, {provide: AuthenticationService, useClass: AuthenticationTestService}]
        });
        const appConfigService = TestBed.inject(AppConfigService);
        appConfigService.loadAppConfig();

        httpTestingController = TestBed.inject(HttpTestingController);
        service = TestBed.inject(LabConfirmService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });
});
