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
  public username:string = ""
  public password:string = ""

  constructor(router: Router) {
    this.router = router;
  }

  public login() {
    // const username = document.querySelector("#exampleInputEmail1").value.toLowerCase()
    console.log(this.username)
    if (this.username.includes("reporter")) {
      this.router.navigate(["icc/report"]);
    } else if (this.username.includes("generator")) {
      this.router.navigate(["icc/generate"]);
    }
  }
}
