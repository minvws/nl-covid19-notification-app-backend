import {IAppConfig} from './app-config.service';

export interface IAppConfigService {
    loadAppConfig();

    getConfig(): IAppConfig;
}
