import {Component} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../environments/environment';
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
    this.generateService.generateDownloadCsv()
      .subscribe((result: any) => {
        var contentDisposition = result.headers.get("content-disposition");
        var filename = (contentDisposition) ? contentDisposition.split(';')[1].split('filename')[1].split('=')[1].trim() : "ICC Batch.csv";
        var bin = new Blob([result.body], {type: 'text/csv'});
        FileSaver.saveAs(bin, filename);
      });
  }

  public downloadCsv() {
    const fileName = "ICC_Batch#" + this.icc_batch.id + '.csv';
    this.generateService.downloadCsv(this.icc_batch.id).subscribe((result) => {
      var bin = new Blob([result], {type: 'text/csv'});
      FileSaver.saveAs(bin, fileName);
    })
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
