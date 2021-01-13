import {Injectable} from '@angular/core';
import {Title} from '@angular/platform-browser';
import { IAppConfig, AppConfigService } from './app-config.service';

@Injectable({
    providedIn: 'root'
})
export class TitleService {
    private titleService: Title;
    private subTitle: string;

    constructor(private readonly service: Title, private readonly appConfigService: AppConfigService) {
        this.titleService = service;
        this.titleService.setTitle(this.appConfigService.getConfig().appName + ' Digitaal Contactonderzoek.');
    }

    public setTitle(title: string) {
        this.subTitle = title;
        this.titleService.setTitle(title + ' – ' + this.appConfigService.getConfig().appName + ' Digitaal Contactonderzoek.');
    }

    public getAppTitle() {
        return this.appConfigService.getConfig().appName;
    }
}
