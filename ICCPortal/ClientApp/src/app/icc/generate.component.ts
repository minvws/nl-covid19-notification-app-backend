import {Component} from '@angular/core';
import {HttpClient} from '@angular/common/http';

@Component({
  selector: 'app-icc',
  templateUrl: './generate.component.html',
  styleUrls: ['./generate.component.css']
})
export class ICCGenerateComponent {

  public icc_code = "123456"
  public all_icc = ""
  private http;

  constructor(http: HttpClient) {
    this.http = http;
    // http.get<WeatherForecast[]>(baseUrl + 'weatherforecast').subscribe(result => {
    //   this.forecasts = result;
    // }, error => console.error(error));
  }


  public generateCode() {
    // const intervaller = setInterval(() => {
    //   this.icc_code = String(Math.floor(Math.random() * (999999 - 100000 + 1)) + 100000);
    // }, 50)
    // setTimeout(() => {
    //   clearInterval(intervaller)
    // }, 500)

    for (let i = 0; i < 11; i++) {
      this.http.get("https://localhost:5005/GenerateICC").subscribe((result) => {
        document.querySelector("input[name='icc_" + i + "']").value = result.icc;
        this.all_icc += result.icc + "\n"
      })
    }

    // TODO: Add API call
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
