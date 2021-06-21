// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import localeNL from '@angular/common/locales/nl';
import {ValidateIccFinalComponent} from './validate-icc-final.component';
import {ActivatedRoute} from '@angular/router';
import {LOCALE_ID} from '@angular/core';
import {registerLocaleData} from '@angular/common';

registerLocaleData(localeNL);

describe('ValidateIccFinalComponent', () => {
    let component: ValidateIccFinalComponent;
    let fixture: ComponentFixture<ValidateIccFinalComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            providers: [
                {
                    provide: ActivatedRoute,
                    useValue: {
                        snapshot: {
                            queryParams: {
                                success: true,
                                symptomsDate: '1597666227190'
                            }
                        },
                    }
                },
                {provide: LOCALE_ID, useValue: 'nl'}
            ],
            declarations: [ValidateIccFinalComponent]
        }).compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(ValidateIccFinalComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });


    it('friendlySymptomsDate() should be in the expected format', () => {
        const result = component.friendlySymptomsDate();
      expect(result).toBe('maandag 17 augustus');
    });
});
