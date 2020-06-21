import {Component} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../environments/environment';
import {ReportService} from "../services/report.service";
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
