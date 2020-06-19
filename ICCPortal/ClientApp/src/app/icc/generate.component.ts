import {Component} from '@angular/core';
import {HttpClient} from '@angular/common/http';

@Component({
  selector: 'app-icc',
  templateUrl: './generate.component.html',
  styleUrls: ['./generate.component.css']
})
export class IccGenerateComponent {
  public all_icc = null
  private http;

  constructor(http: HttpClient) {
    this.http = http;
  }


  public generateCode() {
    this.http.post("https://localhost:5007/GenerateIcc/batch", {"user_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6"}).subscribe((result) => {
      this.all_icc = result.batch;
    })
  }

  public getIccCodeTextArea() {
    return this.all_icc.map((i) => i.code.trim()).join("\n");
  }

  public copyCodeToClipboard() {
    // var textArea = document.createElement("textarea");
    // textArea.value = this.icc_code;
    // textArea.style.top = "0";
    // textArea.style.left = "0";
    // textArea.style.position = "fixed";
    //
    // document.body.appendChild(textArea);
    // textArea.focus();
    // textArea.select();
    //
    // try {
    //   var successful = document.execCommand('copy');
    // } catch (err) {
    // }
    //
    // document.body.removeChild(textArea);
  }
}
