import {Component} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-icc',
  templateUrl: './generate.component.html',
  styleUrls: ['./generate.component.css']
})
export class IccGenerateComponent {
  public icc_batch = null;
  private http;

  constructor(http: HttpClient) {
    this.http = http;
  }

  public generateCode() {
    const serviceUrl = environment.apiUrl + "/GenerateIcc/batch";
    this.http.post(serviceUrl, { "user_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6" })
      .subscribe((result) => {
        this.icc_batch = result.iccBatch;
        alert("Batch #" + result.iccBatch.id + " generated");
      });
  }

  public getIccCodeTextArea() {
    return this.icc_batch.batch.map((i) => i.code.trim()).join("\n");
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
