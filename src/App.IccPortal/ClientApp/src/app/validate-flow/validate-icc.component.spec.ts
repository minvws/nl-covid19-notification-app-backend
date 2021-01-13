import {async, ComponentFixture, TestBed} from '@angular/core/testing';

import {ValidateIccComponent} from './validate-icc.component';
import {HttpClientTestingModule} from '@angular/common/http/testing';
import {RouterTestingModule} from '@angular/router/testing';
import {AppConfigService} from '../services/app-config.service';

describe('ValidateIccComponent', () => {
    let component: ValidateIccComponent;
    let fixture: ComponentFixture<ValidateIccComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule, RouterTestingModule],
            providers: [AppConfigService],
            declarations: [ValidateIccComponent]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(ValidateIccComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
