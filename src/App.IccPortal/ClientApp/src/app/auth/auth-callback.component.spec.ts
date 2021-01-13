import {async, ComponentFixture, TestBed} from '@angular/core/testing';

import {AuthCallbackComponent} from './auth-callback.component';
import {AuthenticationService} from '../services';
import {HttpClientTestingModule} from '@angular/common/http/testing';
import {RouterTestingModule} from '@angular/router/testing';
import {AppConfigService} from '../services/app-config.service';

describe('AuthComponent', () => {
    let component: AuthCallbackComponent;
    let fixture: ComponentFixture<AuthCallbackComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule, RouterTestingModule],
            providers: [AuthenticationService, AppConfigService],
            declarations: [AuthCallbackComponent]
        }).compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(AuthCallbackComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
