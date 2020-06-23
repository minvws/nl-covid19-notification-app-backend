import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from "../../environments/environment";
import { catchError } from 'rxjs/operators';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';

@Injectable({
  providedIn: "root"
})
export class ReportService {

  private authHeader: string = "";

  constructor(private readonly http: HttpClient) { }

  redeemIcc(authHeader: string, labConfirmationIds: Array<string>): Observable<any> {
    this.authHeader = authHeader;
    const serviceUrl = environment.apiUrl + "/RedeemIcc";
    const data = {
      "labConfirmationID": labConfirmationIds.join(""),
      "commencementComplaints": new Date(Date.now()).toISOString()
    };
    const headers = {
      headers: {
        "Authorization": this.authHeader
      }
    }

    return this.http.post(serviceUrl, data, headers).pipe(catchError(this.errorHandler));
  }

  private errorHandler(error: HttpErrorResponse, caught:Observable<any>) : Observable<any> {
    // TODO error handling
    throw error;
  }
}
