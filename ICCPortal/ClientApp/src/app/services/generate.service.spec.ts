import { TestBed } from '@angular/core/testing';

import {GenerateService} from "./generate.service";

describe('GenerateServiceService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: GenerateService = TestBed.get(GenerateService);
    expect(service).toBeTruthy();
  });

});
