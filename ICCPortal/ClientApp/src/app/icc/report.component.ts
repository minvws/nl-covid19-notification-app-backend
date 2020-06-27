import {Component} from '@angular/core';
import {ReportService} from '../services/report.service';

@Component({
  selector: 'app-icc',
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.css']
})
export class IccReportComponent {
  labConfirmationId = ['', '', '', '-', '', '', ''];
  dateOfSymptomsOnset;
  icc = '';

  constructor(private readonly reportService: ReportService) {
    this.dateOfSymptomsOnset = null;
  }

  icIdKeyPress($event: KeyboardEvent, index) {
    const target = $event.target as HTMLInputElement;
    setTimeout(() => {
      console.log(index);
      if ($event.code === 'Backspace') {
        target.value = '';
        (<HTMLInputElement>target.parentNode.querySelector(`.form-control:nth-child(${index})`)).focus();
      } else if (index === 2) {
        (<HTMLInputElement>target.parentNode.querySelector(`.form-control:nth-child(${index + 3})`)).focus();
      } else if (index < 6) {
        console.log(index + ' i');
        (<HTMLInputElement>target.parentNode.querySelector(`.form-control:nth-child(${index + 2})`)).focus();
      } else if (target.parentNode['id'] === 'iccWrapper') {
        (<HTMLInputElement>document.querySelector('#icIdWrapper .form-control:first-child')).focus();
      } else if (target.parentNode['id'] === 'icIdWrapper') {
        (<HTMLInputElement>document.querySelector('.btn:last-child')).focus();
      }
    }, 20);
  }

  report() {
    this.labConfirmationId.forEach((idd, k) => {
      this.labConfirmationId[k] = idd.toUpperCase();
    });
    this.reportService.redeemIcc(this.icc, this.labConfirmationId, this.dateOfSymptomsOnset).subscribe((result) => {
      alert(JSON.stringify(result));
    });

  }
}
