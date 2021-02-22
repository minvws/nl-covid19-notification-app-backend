// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { Component, OnInit } from '@angular/core';
import {CarousselImage} from '../../../models/caroussel-image';

@Component({
    selector: 'app-validate-step1',
    templateUrl: './validate-start-info.component.html',
    styleUrls: ['./validate-start-info.component.scss']
})
export class ValidateStartInfoComponent implements OnInit {

    images: Array<CarousselImage> = [
        {
            text: 'Zet telefoon op luidspreker en open CoronaMelder-app.',
            image: 'assets/images/Screen 0.png',
            small: true
        },
        {
            text: 'Open de app en scroll naar beneden',
            image: 'assets/images/new/1.svg',
            device: true
        },
        {
            text: 'Druk op ‘GGD-sleutel doorgeven’',
            image: 'assets/images/new/2.svg',
            device: true
        },
        {
            text: 'Geef de tijdelijke GGD-sleutel door',
            image: 'assets/images/new/3.svg',
            device: true
        },
    ];

    constructor() {
    }

    ngOnInit(): void {
    }

}
