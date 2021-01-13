import {async, ComponentFixture, TestBed} from '@angular/core/testing';

import {ValidateStartInputComponent} from './validate-start-input.component';
import {RouterTestingModule} from '@angular/router/testing';
import {HttpClientTestingModule} from '@angular/common/http/testing';
import {AppConfigService} from '../../../services/app-config.service';

describe('ValidateStartInputComponent', () => {
    let component: ValidateStartInputComponent;
    let fixture: ComponentFixture<ValidateStartInputComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            providers: [
                AppConfigService
            ],
            imports: [RouterTestingModule.withRoutes([]), HttpClientTestingModule],
            declarations: [ValidateStartInputComponent]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(ValidateStartInputComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('getDayAgo returns date on x days -2 ago', () => {
        const daysInPast = 2;
        // testdate = 31 dec 2020
        const inputDate = new Date(Date.UTC(2020, 0, 3, 4, 2, 2));

        const expectedDate = new Date(Date.UTC(2020, 0, 1, 0, 0, 0, 0));
        const result = component.getDayAgo(daysInPast, inputDate);

        expect(result).toEqual(expectedDate);
    });
    it('getDayAgo returns date on x days -2 ago ny eve', () => {
        const daysInPast = 2;
        // testdate = 31 dec 2020
        const inputDate = new Date(Date.UTC(2020, 0, 2, 4, 2, 2));

        const expectedDate = new Date(Date.UTC(2019, 11, 31, 0, 0, 0, 0));
        const result = component.getDayAgo(daysInPast, inputDate);

        expect(result).toEqual(expectedDate);
    });

    it('getDayAgo returns date on x days -2 ago gmt +1', () => {
        const daysInPast = 2;
        // testdate = 31 dec 2020
        const inputDate = new Date(Date.UTC(2020, 11, 31, 4, 2, 2));

        const expectedDate = new Date(Date.UTC(2020, 11, 29, 0, 0, 0, 0));
        const result = component.getDayAgo(daysInPast, inputDate);

        expect(result).toEqual(expectedDate);
    });

    it('getDayAgo returns date on x days -2 ago gmt +2', () => {
        const daysInPast = 2;
        // testdate = 6 sep 2020
        const inputDate = new Date(Date.UTC(2020, 6, 3, 4, 2, 2));

        const expectedDate = new Date(Date.UTC(2020, 6, 1, 0, 0, 0, 0));
        const result = component.getDayAgo(daysInPast, inputDate);

        expect(result).toEqual(expectedDate);
    });
});
