import {Component} from '@angular/core';
import {Element} from "@angular/compiler";
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-icc',
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.css']
})
export class ICCReportComponent {
  public icc_code = "123456"
  private http

  constructor(http: HttpClient) {
    this.http = http
  }

  public ICIdKeyPress($event: KeyboardEvent, index) {
    // let target = $event.target
    // setTimeout(() => {
    //   if ($event.keyCode == 8) {
    //     target.value = ""
    //     target.parentNode.querySelector(".form-control:nth-child(" + (index) + ")").focus()
    //   } else if (index < 5) {
    //     target.parentNode.querySelector(".form-control:nth-child(" + (index + 2) + ")").focus()
    //   } else if (target.parentNode.id == 'iccWrapper') {
    //     document.querySelector("#icIdWrapper .form-control:first-child").focus()
    //   } else if (target.parentNode.id == 'icIdWrapper') {
    //     document.querySelector(".btn:last-child").focus()
    //   }
    // })
  }

  public report() {


    // var ICC = document.querySelector("#iccWrapper .form-control").value
    // var ICId = Array.from(document.querySelectorAll("#icIdWrapper .form-control")).map(el => el.value).join("")
    //
    // this.http.post("https://localhost:5005/RedeemICC", {
    //   icc: ICC,
    //   ICC_Id: ICId
    // }).subscribe((result) => {
    //   alert(JSON.stringify(result))
    // });


    // console.log(ICId);
    // console.log(ICC);
    // setTimeout(() => {
    //   Array.from(document.querySelectorAll(".form-control")).forEach(el => {
    //     el.value = ""
    //   })
    // }, 200)
  }
}
