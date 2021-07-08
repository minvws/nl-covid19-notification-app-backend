// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { enableProdMode, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IAppConfigService } from './i-app-config.service';

@Injectable()
export class AppConfigService implements IAppConfigService {
  protected appConfig: IAppConfig;

  constructor(private http: HttpClient) {
  }

  async loadAppConfig() {
    const data = await this.http.get('/assets/data/appConfig.json').toPromise();
    this.appConfig = <IAppConfig>data;
  }

  getConfig(): IAppConfig {
    return this.appConfig;
  }
}

export interface IAppConfig {
  appName: string;
  authHost: string;
  symptomaticIndexDayOffset: number;
  aSymptomaticIndexDayOffset: number;
}
