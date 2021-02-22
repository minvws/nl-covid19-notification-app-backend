// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { enableProdMode, Injectable } from '@angular/core';
import {AppConfigService, IAppConfig} from './app-config.service';

@Injectable()
export class AppConfigTestService extends AppConfigService {

    async loadAppConfig() {
        this.appConfig = <IAppConfig>{
            appName: 'CoronaMelder TestSuite',
            authHost: 'coronamelder.test'
        };
    }
}

