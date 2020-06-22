import {Component} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import { environment } from '../../environments/environment';
import * as FileSaver from 'file-saver';
import {GenerateService} from "../services/generate.service";

@Component({
  selector: 'app-icc',
  templateUrl: './generate.component.html',
  styleUrls: ['./generate.component.css']
})
export class IccGenerateComponent {
  public icc_batch = null;

  constructor(private readonly generateService: GenerateService) {
  }

  public generateCode() {

    this.generateService.generateIccBatch()
      .subscribe((result) => {
        this.icc_batch = result.iccBatch;
        alert("Batch #" + result.iccBatch.id + " generated");
      });
  }

  public generateDownloadCsv() {
    const serviceUrl = environment.apiUrl + "/GenerateIcc/batch-csv";
    this.http
      .post(serviceUrl, { "user_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }, { responseType: 'blob' })
      .subscribe((result) => {
        var bin = new Blob([result], { type: 'text/csv' });
        console.log("wanna see the contentdisposition value");
        FileSaver.saveAs(bin, 'icc.csv');
      });
  }

  public downloadCsv() {
    const serviceUrl = environment.apiUrl + "/GenerateIcc/batch-csv?batchId="+this.icc_batch.id;
    const fileName = this.icc_batch.id + '.csv';

    this.http
      .get(serviceUrl, { responseType: 'blob' })
      .subscribe((result) => {
        var bin = new Blob([result], { type: 'text/csv' });
        FileSaver.saveAs(bin, fileName);
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
