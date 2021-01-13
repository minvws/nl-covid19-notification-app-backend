import {Component, HostListener, OnInit} from '@angular/core';
import {TitleService} from '../services/title.service';
import {ActivatedRoute} from '@angular/router';
import {AuthenticationService} from '../services';

@Component({
    selector: 'app-validate-icc',
    templateUrl: './validate-icc.component.html',
    styleUrls: ['./validate-icc.component.scss']
})
export class ValidateIccComponent implements OnInit {

    public InfectionConfirmationId: Array<string> = ['', '', '', '', '', ''];
    public InfectionConfirmationIdIsValid = false;
    public IndexIsSubmitted = false;
    public scrollDown = false;

    constructor(public authenticationService: AuthenticationService) {
        window.addEventListener('beforeunload', (event) => {
            event.preventDefault();
            event.returnValue =  'Weet je zeker dat je wilt herladen? De ingevoerde sleutel gaat verloren.';
            return event;
        });
    }

    @HostListener('window:scroll', ['$event'])
    scrollHandler(event) {
        if (window.scrollY > 40) {
            this.scrollDown = true;
        } else {
            this.scrollDown = false;
        }
    }
    ngOnInit(): void {
    }
}
