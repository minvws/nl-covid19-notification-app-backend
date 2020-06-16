import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import {ICCReportComponent} from "./icc/report.component";
import {ICCGenerateComponent} from "./icc/generate.component";
import {LoginComponent} from "./login/login.component";




@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    ICCReportComponent,
    ICCGenerateComponent,
    LoginComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'login', component: LoginComponent, pathMatch: 'full' },
      { path: 'icc/report', component: ICCReportComponent, pathMatch: 'full' },
      { path: 'icc/generate', component: ICCGenerateComponent, pathMatch: 'full' },
    ])
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
