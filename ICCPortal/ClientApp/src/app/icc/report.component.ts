import {Component} from '@angular/core';
import {Element} from "@angular/compiler";
import {HttpClient} from "@angular/common/http";
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-icc',
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.css']
})
export class IccReportComponent {
  public Icc: string = ""
  public labConfirmationID = ["", "", "", "", "", ""]
  private http

  constructor(http: HttpClient) {
    this.http = http
  }

  public ICIdKeyPress($event: KeyboardEvent, index) {
    let target:HTMLInputElement = (<HTMLInputElement>$event.target)
    setTimeout(() => {
      if ($event.keyCode == 8) {
        target.value = "";
        (<HTMLInputElement>target.parentNode.querySelector(".form-control:nth-child(" + (index) + ")")).focus();
      } else if (index < 5) {
        (<HTMLInputElement>target.parentNode.querySelector(".form-control:nth-child(" + (index + 2) + ")")).focus();
      } else if (target.parentNode['id'] == 'iccWrapper') {
        (<HTMLInputElement>document.querySelector("#icIdWrapper .form-control:first-child")).focus();
      } else if (target.parentNode['id'] == 'icIdWrapper') {
        (<HTMLInputElement>document.querySelector(".btn:last-child")).focus();
      }
    },200);
  }
  public report() {


    // var Icc = document.querySelector("#iccWrapper .form-control").value
    // var ICId = Array.from(document.querySelectorAll("#icIdWrapper .form-control")).map(el => el.value).join("")
    //
    const serviceUrl = environment.apiUrl + "/RedeemIcc";
    this.http.post(serviceUrl, {
      "labConfirmationID": this.labConfirmationID.join(""),
      "commencementComplaints": new Date(Date.now()).toISOString()
    }, {
      headers: {
        "Authorization": this.Icc
      }
    }).subscribe((result) => {
      alert(JSON.stringify(result))
    });


    // console.log(ICId);
    // console.log(Icc);
    // setTimeout(() => {
    //   Array.from(document.querySelectorAll(".form-control")).forEach(el => {
    //     el.value = ""
    //   })
    // }, 200)
  }
}
