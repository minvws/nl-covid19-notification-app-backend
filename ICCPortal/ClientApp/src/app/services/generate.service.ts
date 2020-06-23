import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {environment} from "../../environments/environment";
import {catchError} from 'rxjs/operators';
import {HttpClient, HttpErrorResponse} from '@angular/common/http';
import * as FileSaver from "file-saver";

@Injectable({
  providedIn: "root"
})
export class GenerateService {

  private readonly authHeader: string = "";
  private readonly testUserId: string = "3fa85f64-5717-4562-b3fc-2c963f66afa6"; // TODO: Create global state for user management

  constructor(private readonly http: HttpClient) {
  }

  generateIccSingle(): Observable<any> {
    const serviceUrl = environment.apiUrl + "/GenerateIcc/single";

    let payload = {
      UserId: this.testUserId
    }
    return this.http.post(serviceUrl, payload).pipe(catchError(this.errorHandler));
  }

  generateIccBatch(): Observable<any> {
    const serviceUrl = environment.apiUrl + "/GenerateIcc/batch";

    let payload = {
      UserId: this.testUserId
    }
    return this.http.post(serviceUrl, payload).pipe(catchError(this.errorHandler));
  }


  generateDownloadCsv(): Observable<any> {
    const serviceUrl = environment.apiUrl + "/GenerateIcc/batch-csv";
    return this.http.post(serviceUrl, { "UserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }, {
      observe: 'response',
      responseType: 'blob' as 'json'
    })
  }

  downloadCsv(icc_batch_id) {
    const serviceUrl = environment.apiUrl + "/GenerateIcc/batch-csv?batchId="+icc_batch_id;
    return this.http.get(serviceUrl, { responseType: 'blob' });
  }

  private errorHandler(error: HttpErrorResponse, caught: Observable<any>): Observable<any> {
    // TODO: this retries, implement proper error handling / logging here.
    // return caught;
  }
}
