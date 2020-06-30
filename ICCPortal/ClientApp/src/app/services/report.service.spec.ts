import {TestBed} from '@angular/core/testing';

import {ReportService} from './report.service';
import {HttpClient, HttpHandler} from '@angular/common/http';

describe('ReportServiceService', () => {
  let service: ReportService;
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ReportService, HttpClient, HttpHandler]
    });
    service = TestBed.inject(ReportService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
