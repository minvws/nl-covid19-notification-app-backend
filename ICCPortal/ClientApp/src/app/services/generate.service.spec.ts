import {TestBed} from '@angular/core/testing';
import {GenerateService} from './generate.service';
import {HttpClientTestingModule, HttpTestingController} from '@angular/common/http/testing';

describe('GenerateServiceService', () => {
  let service: GenerateService;
  let httpTestingController: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [GenerateService],
      imports: [HttpClientTestingModule]
    });
    httpTestingController = TestBed.get(HttpTestingController);
    service = TestBed.get(GenerateService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('returned Observable should match the right data', () => {
    const mockIccBatch = {
      ok: true,
      status: 200,
      length: 4,
      iccBatch: {
        id: '1234AB',
        batch: [{
          code: 'A12345678910112',
        }, {
          code: 'A12345678910112',
        }, {
          code: 'A12345678910112',
        }, {
          code: 'A12345678910112',
        }]
      }
    };
    service.generateIccBatch().subscribe(result => {
      console.log(result);
      expect(result.ok).toEqual(true);
      expect(result.status).toEqual(200);
      expect(result.length).toEqual(4);
      expect(result.icc_batch.length()).toEqual(4);
      expect(result.icc_batch[0].code).toEqual('A12345678910112');
    });
    const req = httpTestingController.expectOne('http://localhost:5006/GenerateIcc/batch');

    req.flush(mockIccBatch);
  });


});
