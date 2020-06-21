import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {environment} from "../../environments/environment";
import {catchError} from 'rxjs/operators';
import {HttpClient, HttpErrorResponse} from '@angular/common/http';

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
      user_id: this.testUserId
    }
    return this.http.post(serviceUrl, payload).pipe(catchError(this.errorHandler));
  }

  generateIccBatch(): Observable<any> {
    const serviceUrl = environment.apiUrl + "/GenerateIcc/batch";

    let payload = {
      user_id: this.testUserId
    }
    return this.http.post(serviceUrl, payload).pipe(catchError(this.errorHandler));
  }

  private errorHandler(error: HttpErrorResponse, caught: Observable<any>): Observable<any> {
    // TODO: this retries, implement proper error handling / logging here.
    return caught;
  }
}
