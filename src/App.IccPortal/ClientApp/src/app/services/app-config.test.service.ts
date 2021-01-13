import {enableProdMode, Injectable} from '@angular/core';
import {AppConfigService, IAppConfig} from './app-config.service';

@Injectable()
export class AppConfigTestService extends AppConfigService {

    async loadAppConfig() {
        this.appConfig = <IAppConfig>{
            appName: 'CoronaMelder TestSuite',
            apiUrl: 'http://coronamelder.test',
            authHost: 'coronamelder.test'
        };
    }
}

