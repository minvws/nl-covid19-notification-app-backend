// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import localeNL from '@angular/common/locales/nl';
import { ValidateStartInputComponent } from './validate-start-input.component';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { AppConfigService } from '../../../services/app-config.service';
import { DateHelper } from '../../../helpers/date.helper';

import { LOCALE_ID } from '@angular/core';
import { registerLocaleData } from '@angular/common';

registerLocaleData(localeNL);

class MockAppConfigService {
  appName: 'IccBackend';
  symptomaticIndexDayOffset: 0;
  aSymptomaticIndexDayOffset: 2;

  getConfig() {
    return this;
  }
}

describe('ValidateStartInputComponent', () => {
  let component: ValidateStartInputComponent;
  let fixture: ComponentFixture<ValidateStartInputComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
        providers: [
          { provide: AppConfigService, useClass: MockAppConfigService },
          { provide: LOCALE_ID, useValue: 'nl' },
          { provide: DateHelper }
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
});
