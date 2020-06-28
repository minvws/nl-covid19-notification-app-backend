import {Component} from '@angular/core';
import * as FileSaver from 'file-saver';
import {GenerateService} from '../services/generate.service';

@Component({
  selector: 'app-icc',
  templateUrl: './generate.component.html',
  styleUrls: ['./generate.component.css']
})
export class IccGenerateComponent {
  iccBatch = null;

  constructor(private readonly generateService: GenerateService) {
  }

  generateCode() {
    this.generateService.generateIccBatch()
      .subscribe((result) => {
        this.iccBatch = result.iccBatch;
        alert(`Batch #${result.iccBatch.id} generated`);
      });
  }

  generateDownloadCsv() {
    this.generateService.generateDownloadCsv()
      .subscribe((result: any) => {
        const contentDisposition = result.headers.get('content-disposition');
        const filename = (contentDisposition) ?
            contentDisposition.split(';')[1].split('filename')[1].split('=')[1].trim() :
            'ICC Batch.csv';
        const bin = new Blob([result.body], {type: 'text/csv'});
        FileSaver.saveAs(bin, filename);
      });
  }

  downloadCsv() {
    const fileName = `ICC_Batch#${this.iccBatch.id}.csv`;
    this.generateService.downloadCsv(this.iccBatch.id).subscribe((result) => {
      const bin = new Blob([result], {type: 'text/csv'});
      FileSaver.saveAs(bin, fileName);
    });
  }

  getIccCodeTextArea() {
    return this.iccBatch.batch.map((i) => i.code.trim()).join('\n');
  }

  copyCodeToClipboard() {
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
