import {TestBed} from '@angular/core/testing';
import {HttpClientTestingModule, HttpTestingController} from '@angular/common/http/testing';
import {RouterTestingModule} from '@angular/router/testing';
import {AppConfigTestService} from './app-config.test.service';
import {AppConfigService} from './app-config.service';
import {AuthenticationTestService} from './authentication_test.service';
import {AuthenticationService} from './authentication.service';

describe('AuthenticationServiceService', () => {
    let service: AuthenticationService;
    let httpTestingController: HttpTestingController;
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule, RouterTestingModule],
            providers: [AuthenticationService, {
                provide: AppConfigService,
                useClass: AppConfigTestService,
            }, {provide: AuthenticationService, useClass: AuthenticationTestService}
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


    it('currentUserValue.authData should return correct test user', () => {
        const expectedAuthData = 'authData';

        const result = service.currentUserValue.authData;

        expect(result).toEqual(expectedAuthData);
    });

    it('buildUrl should build correct url', () => {
        const expected = 'http://localhost:9876/Auth/User';

        const result = service.buildUrl('Auth/User');

        expect(result).toEqual(expected);
    });
});

