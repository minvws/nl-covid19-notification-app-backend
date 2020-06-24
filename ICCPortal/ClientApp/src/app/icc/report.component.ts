import {Component} from '@angular/core';
import {ReportService} from "../services/report.service";

@Component({
  selector: 'app-icc',
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.css']
})
export class IccReportComponent {
  public labConfirmationId = ["", "", "-", "", "", "-", "", ""];
  public dateOfSymptomsOnset;
  public icc = "";

  constructor(private readonly reportService: ReportService) {
    this.dateOfSymptomsOnset = null;
  }

  public ICIdKeyPress($event: KeyboardEvent, index) {
    let target: HTMLInputElement = (<HTMLInputElement>$event.target);
    setTimeout(() => {
      if ($event.keyCode === 8) {
        target.value = "";
        (<HTMLInputElement>target.parentNode.querySelector(".form-control:nth-child(" + (index) + ")")).focus();
      } else if (index < 7) {
        (<HTMLInputElement>target.parentNode.querySelector(".form-control:nth-child(" + (index + 2) + ")")).focus();
      } else if (target.parentNode['id'] === 'iccWrapper') {
        (<HTMLInputElement>document.querySelector("#icIdWrapper .form-control:first-child")).focus();
      } else if (target.parentNode['id'] === 'icIdWrapper') {
        (<HTMLInputElement>document.querySelector(".btn:last-child")).focus();
      }
    }, 50);
  }

  public report() {

    this.reportService.redeemIcc(this.icc, this.labConfirmationId, this.dateOfSymptomsOnset).subscribe((result) => {
      alert(JSON.stringify(result));
    });

  }
}
