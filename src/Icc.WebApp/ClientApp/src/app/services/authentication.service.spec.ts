// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { TestBed } from '@angular/core/testing';
import {HttpClientTestingModule, HttpTestingController} from '@angular/common/http/testing';
import {RouterTestingModule} from '@angular/router/testing';
import {AppConfigTestService} from './app-config.test.service';
import {AppConfigService} from './app-config.service';
import {AuthenticationService} from './authentication.service';

describe('AuthenticationServiceService', () => {
    let service: AuthenticationService;
    let httpTestingController: HttpTestingController;

    // {
    //     "exp": 1732924800,
    //     "id": 1,
    //     "access_token": "test_access_token",
    //     "name": "Test User"
    // }
    const testJwtToken = 'eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOjE3MzI5MjQ4MDAsImlkIjoxLCJhY2Nlc3NfdG9rZW4iOiJ0ZXN0X2FjY2Vzc190b2tlbiIsIm5hbWUiOiJUZXN0IFVzZXIifQ.cHIu_HMVgKaBb30WM5P7LxQ_Qe5LVgdIT7xzZSQHO3c';

    const {location} = window;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule, RouterTestingModule],
            providers: [AuthenticationService, {
                provide: AppConfigService,
                useClass: AppConfigTestService,
            }, AuthenticationService
            ],
        });
        const appConfigService = TestBed.inject(AppConfigService);
        appConfigService.loadAppConfig();

        httpTestingController = TestBed.inject(HttpTestingController);
        service = TestBed.inject(AuthenticationService);
    });


    it('should be created', () => {
        expect(service).toBeTruthy();
    });


    it('callback should set localStorage', () => {
        const mockResponse = {
            token: testJwtToken
        };
        service.callback('test_auth_code').subscribe((r) => {

        });

      const req = httpTestingController.expectOne('https://coronamelder.test/Auth/Token');
        req.flush(mockResponse);

        expect(localStorage.getItem('auth')).toEqual(testJwtToken);
    });

    it('callback should sets correct userobject', () => {

        const mockResponse = {
            token: testJwtToken
        };

        service.callback('test_auth_code').subscribe();

      const req = httpTestingController.expectOne('https://coronamelder.test/Auth/Token');
        req.flush(mockResponse);

        expect(service.currentUserValue.displayName).toEqual('Test User');
        expect(service.currentUserValue.authData).toEqual(testJwtToken);
        expect(service.currentUserValue.id).toEqual(1);
    });


    it('logout should reset the localStorage', () => {

        // Arrange
        const mockResponse = {
            token: testJwtToken
        };
        const expected = null;
      const targetUrl = 'https://coronamelder.test/Auth/Logout';

        // run callback to fill localStorage

        service.callback('test_auth_code').subscribe((r) => {

        });

      const req = httpTestingController.expectOne('https://coronamelder.test/Auth/Token');
        req.flush(mockResponse);

        // Act
        service.logout(false);

        const result = localStorage.getItem('auth');

        // Assert
        expect(result).toEqual(expected);
        expect(service.currentUserValue).toEqual(expected);

    });
});

