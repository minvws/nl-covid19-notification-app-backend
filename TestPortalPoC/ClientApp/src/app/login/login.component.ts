import {Component} from '@angular/core';
import {Element} from "@angular/compiler";
import {Router} from "@angular/router";

@Component({
  selector: 'app-icc',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  private router

  constructor(router: Router) {
    this.router = router;
  }

  public login() {
    const username = document.querySelector("#exampleInputEmail1").value.toLowerCase()
    if (username.includes("reporter")) {
      this.router.navigate(["icc/report"]);
    } else if (username.includes("generator")){
      this.router.navigate(["icc/generate"]);
    }
  }
}
